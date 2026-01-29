(() => {
    const state = {
        tiendas: [],
        conceptos: [],
        motivos: [],
        itemsCache: [],
        rows: [],
        docEntry: '',
        pending: new Set(),
        rowSeq: 1,
        lastPeriod: '',
        lastMotivo: 'TODOS'
    };

    const elements = {
        periodo: document.getElementById('filtroPeriodo'),
        tiendasEstado: document.getElementById('tiendasEstado'),
        tiendasListado: document.getElementById('tiendasListado'),
        btnSelTodas: document.getElementById('tiendaSelTodas'),
        btnLimpiar: document.getElementById('tiendaLimpiar'),
        filtroMotivo: document.getElementById('filtroMotivo'),
        btnBuscar: document.getElementById('btnBuscar'),
        btnNuevo: document.getElementById('btnNuevo'),
        btnActualizar: document.getElementById('btnActualizarTodo'),
        tablaBody: document.querySelector('#tablaPrevision tbody'),
        alertContainer: document.getElementById('alertContainer'),
        loadingOverlay: document.getElementById('loadingOverlay'),
        loadingMessage: document.getElementById('loadingMessage'),
        pendingBadge: document.getElementById('pendingBadge'),
        btnPeriodoAnterior: document.getElementById('btnPeriodoAnterior'),
        btnPeriodoSiguiente: document.getElementById('btnPeriodoSiguiente'),
        modalCambios: document.getElementById('modalCambios'),
        btnDescartar: document.getElementById('btnDescartar'),
        listaItems: document.getElementById('listaItems')
    };

    let modalConfirm = null;
    let pendingAction = null;

    const init = () => {
        state.lastPeriod = elements.periodo.value;
        state.lastMotivo = 'TODOS';

        modalConfirm = new bootstrap.Modal(elements.modalCambios);

        elements.btnDescartar.addEventListener('click', () => {
            clearPending();
            modalConfirm.hide();
            if (pendingAction) {
                const action = pendingAction;
                pendingAction = null;
                action();
            }
        });

        elements.periodo.addEventListener('change', () => handleFilterChange('periodo'));
        elements.filtroMotivo.addEventListener('change', () => handleFilterChange('motivo'));

        elements.btnBuscar.addEventListener('click', () => handleActionWithPending(buscar));
        elements.btnNuevo.addEventListener('click', () => handleActionWithPending(agregarLinea));
        elements.btnActualizar.addEventListener('click', () => handleActionWithPending(guardarTodo));

        elements.btnSelTodas.addEventListener('click', (event) => {
            event.preventDefault();
            clearAlerts();
            const checks = elements.tiendasListado.querySelectorAll('input[type="checkbox"]');
            const shouldConfirm = state.pending.size > 0;
            if (shouldConfirm) {
                requestDiscard(() => {
                    checks.forEach(chk => { chk.checked = true; });
                    updateSelectedTiendas();
                });
                return;
            }
            checks.forEach(chk => { chk.checked = true; });
            updateSelectedTiendas();
        });

        elements.btnLimpiar.addEventListener('click', (event) => {
            event.preventDefault();
            clearAlerts();
            const checks = elements.tiendasListado.querySelectorAll('input[type="checkbox"]');
            const shouldConfirm = state.pending.size > 0;
            if (shouldConfirm) {
                requestDiscard(() => {
                    checks.forEach(chk => { chk.checked = false; });
                    updateSelectedTiendas();
                });
                return;
            }
            checks.forEach(chk => { chk.checked = false; });
            updateSelectedTiendas();
        });

        elements.tiendasListado.addEventListener('change', (event) => {
            const target = event.target;
            if (target.matches('input[type="checkbox"]')) {
                clearAlerts();
                if (state.pending.size > 0) {
                    const original = !target.checked;
                    requestDiscard(() => {
                        updateSelectedTiendas();
                    }, () => {
                        target.checked = original;
                    });
                    return;
                }
                updateSelectedTiendas();
            }
        });

        elements.btnPeriodoAnterior.addEventListener('click', () => cambiarPeriodo(-1));
        elements.btnPeriodoSiguiente.addEventListener('click', () => cambiarPeriodo(1));

        elements.tablaBody.addEventListener('change', handleTableChange);
        elements.tablaBody.addEventListener('blur', handleTableBlur, true);
        elements.tablaBody.addEventListener('input', handleItemSearch);

        window.addEventListener('beforeunload', (event) => {
            if (state.pending.size > 0) {
                event.preventDefault();
                event.returnValue = '';
            }
        });

        document.body.addEventListener('click', (event) => {
            const link = event.target.closest('a');
            if (!link || !link.href || link.getAttribute('href') === '#') {
                return;
            }

            if (state.pending.size > 0) {
                event.preventDefault();
                requestDiscard(() => {
                    window.location.href = link.href;
                });
            }
        });

        cargarInicial();
    };

    const handleActionWithPending = (action) => {
        clearAlerts();
        if (state.pending.size > 0 && action !== agregarLinea) {
            requestDiscard(action);
        } else {
            action();
        }
    };

    const requestDiscard = (action, onCancel) => {
        pendingAction = action;
        modalConfirm.show();
        elements.modalCambios.addEventListener('hidden.bs.modal', () => {
            if (pendingAction === action) {
                pendingAction = null;
                if (onCancel) {
                    onCancel();
                }
            }
        }, { once: true });
    };

    const handleFilterChange = (tipo) => {
        clearAlerts();
        if (state.pending.size > 0) {
            const previous = tipo === 'periodo' ? state.lastPeriod : state.lastMotivo;
            requestDiscard(() => {
                if (tipo === 'periodo') {
                    state.lastPeriod = elements.periodo.value;
                } else {
                    state.lastMotivo = elements.filtroMotivo.value;
                }
            }, () => {
                if (tipo === 'periodo') {
                    elements.periodo.value = previous;
                } else {
                    elements.filtroMotivo.value = previous;
                }
            });
            return;
        }

        if (tipo === 'periodo') {
            state.lastPeriod = elements.periodo.value;
        } else {
            state.lastMotivo = elements.filtroMotivo.value;
        }
    };

    const cargarInicial = async () => {
        await Promise.all([cargarTiendas(), cargarConceptos(), cargarMotivos()]);
    };

    const cargarTiendas = async () => {
        try {
            const resp = await fetchJson('/PrevisionGastos/Tiendas');
            state.tiendas = resp || [];
            renderTiendas();
        } catch (error) {
            showAlert('danger', 'No se pudieron cargar las tiendas.');
        }
    };

    const cargarConceptos = async () => {
        try {
            const resp = await fetchJson('/PrevisionGastos/ConceptosPrm');
            state.conceptos = resp || [];
        } catch (error) {
            showAlert('danger', 'No se pudieron cargar los conceptos PRM.');
        }
    };

    const cargarMotivos = async () => {
        try {
            const resp = await fetchJson('/PrevisionGastos/MotivosGasto');
            state.motivos = resp || [];
            renderMotivos();
        } catch (error) {
            showAlert('danger', 'No se pudieron cargar los motivos de gasto.');
        }
    };

    const renderTiendas = () => {
        if (!elements.tiendasListado) return;
        elements.tiendasListado.innerHTML = '';
        if (state.tiendas.length === 0) {
            elements.tiendasEstado.textContent = 'No hay tiendas disponibles.';
            return;
        }

        elements.tiendasEstado.textContent = '';
        state.tiendas.forEach(tienda => {
            const id = `chk-${tienda.PrjCode}`;
            const wrapper = document.createElement('div');
            wrapper.className = 'form-check';
            wrapper.innerHTML = `
                <input class="form-check-input" type="checkbox" value="${tienda.PrjCode}" id="${id}">
                <label class="form-check-label" for="${id}">${tienda.PrjCode} - ${tienda.PrjName}</label>
            `;
            elements.tiendasListado.appendChild(wrapper);
        });
    };

    const renderMotivos = () => {
        elements.filtroMotivo.innerHTML = '';
        const optionTodos = document.createElement('option');
        optionTodos.value = 'TODOS';
        optionTodos.textContent = 'TODOS';
        elements.filtroMotivo.appendChild(optionTodos);

        state.motivos.forEach(motivo => {
            const option = document.createElement('option');
            option.value = motivo.Code;
            option.textContent = `${motivo.Code} - ${motivo.Name}`;
            elements.filtroMotivo.appendChild(option);
        });
    };

    const updateSelectedTiendas = () => {
        const selected = Array.from(elements.tiendasListado.querySelectorAll('input[type="checkbox"]:checked'))
            .map(chk => chk.value);
        state.selectedTiendas = new Set(selected);
    };

    const buscar = async () => {
        if (!navigator.onLine) {
            showAlert('warning', 'No tiene conexión. Vuelva a intentar.');
            return;
        }

        const periodo = elements.periodo.value;
        if (!periodo) {
            showAlert('danger', 'Debe seleccionar un periodo.');
            return;
        }

        const tiendas = Array.from(elements.tiendasListado.querySelectorAll('input[type="checkbox"]:checked'))
            .map(chk => chk.value)
            .join(',');

        const motivo = elements.filtroMotivo.value || 'TODOS';

        setLoading(true, 'Buscando...');
        try {
            const resp = await fetchJson(`/PrevisionGastos/Buscar?periodo=${encodeURIComponent(periodo)}&tiendas=${encodeURIComponent(tiendas)}&motivo=${encodeURIComponent(motivo)}`);
            state.docEntry = resp?.DocEntry || '';
            state.rows = (resp?.Items || []).map(item => ({
                ...item,
                rowKey: state.rowSeq++,
                isNew: false
            }));
            renderTable();
            clearPending();
        } catch (error) {
            showAlert('danger', 'No se pudo cargar la información.');
        } finally {
            setLoading(false);
        }
    };

    const agregarLinea = () => {
        const periodo = elements.periodo.value;
        if (!periodo) {
            showAlert('danger', 'Debe seleccionar un periodo.');
            return;
        }

        const tiendasSeleccionadas = Array.from(elements.tiendasListado.querySelectorAll('input[type="checkbox"]:checked'))
            .map(chk => chk.value);
        const tiendaDefault = tiendasSeleccionadas[0] || '';
        const fechaDefault = defaultFechaPeriodo(periodo);

        const nueva = {
            rowKey: state.rowSeq++,
            LineId: '',
            U_MGS_CL_TIENDA: tiendaDefault,
            U_MGS_CL_CONPRM: '',
            U_MGS_CL_TIPMOP: '',
            U_MGS_CL_ITEMCOD: '',
            U_MGS_CL_FECHA: fechaDefault,
            U_MGS_CL_IMPORT: 0,
            U_MGS_CL_VALIDO: 'NO',
            isNew: true
        };

        state.rows.push(nueva);
        renderTable();
        markPending(nueva.rowKey);
    };

    const renderTable = () => {
        elements.tablaBody.innerHTML = '';
        if (state.rows.length === 0) {
            return;
        }

        state.rows.forEach((row, index) => {
            const tr = document.createElement('tr');
            tr.dataset.rowKey = row.rowKey;
            if (state.pending.has(row.rowKey)) {
                tr.classList.add('fila-pendiente');
            }

            const tiendaOptions = buildOptions(state.tiendas.map(t => ({
                value: t.PrjCode,
                label: `${t.PrjCode} - ${t.PrjName}`
            })), row.U_MGS_CL_TIENDA);

            const conceptoOptions = buildOptions(state.conceptos.map(c => ({
                value: c.Code,
                label: `${c.Code} - ${c.Name}`
            })), row.U_MGS_CL_CONPRM);

            const motivoOptions = buildOptions(state.motivos.map(m => ({
                value: m.Code,
                label: `${m.Code} - ${m.Name}`
            })), row.U_MGS_CL_TIPMOP);

            const validoOptions = buildOptions([
                { value: 'NO', label: 'NO' },
                { value: 'SI', label: 'SI' }
            ], row.U_MGS_CL_VALIDO || 'NO');

            tr.innerHTML = `
                <td>${index + 1}</td>
                <td class="sticky-col-start">
                    <select class="form-select form-select-sm select-inline" data-field="U_MGS_CL_TIENDA">
                        ${tiendaOptions}
                    </select>
                </td>
                <td>
                    <select class="form-select form-select-sm select-inline" data-field="U_MGS_CL_CONPRM">
                        ${conceptoOptions}
                    </select>
                </td>
                <td>
                    <select class="form-select form-select-sm select-inline" data-field="U_MGS_CL_TIPMOP">
                        ${motivoOptions}
                    </select>
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm input-inline input-item" list="listaItems" data-field="U_MGS_CL_ITEMCOD" value="${escapeHtml(row.U_MGS_CL_ITEMCOD || '')}" autocomplete="off" />
                </td>
                <td>
                    <input type="date" class="form-control form-control-sm input-inline" data-field="U_MGS_CL_FECHA" value="${escapeHtml(row.U_MGS_CL_FECHA || '')}" />
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm input-inline input-importe" data-field="U_MGS_CL_IMPORT" value="${formatAmount(row.U_MGS_CL_IMPORT)}" />
                </td>
                <td>
                    <select class="form-select form-select-sm select-inline" data-field="U_MGS_CL_VALIDO">
                        ${validoOptions}
                    </select>
                </td>
            `;

            elements.tablaBody.appendChild(tr);

            const fechaInput = tr.querySelector('[data-field="U_MGS_CL_FECHA"]');
            if (!isFechaValida(row.U_MGS_CL_FECHA)) {
                fechaInput.classList.add('celda-invalida');
            }

            const importeInput = tr.querySelector('[data-field="U_MGS_CL_IMPORT"]');
            if (isNaN(Number(row.U_MGS_CL_IMPORT))) {
                importeInput.classList.add('celda-invalida');
            }
        });
    };

    const buildOptions = (items, selectedValue) => {
        const emptyOption = '<option value=""></option>';
        const options = items.map(item => {
            const selected = item.value === selectedValue ? 'selected' : '';
            return `<option value="${item.value}" ${selected}>${item.label}</option>`;
        });
        return emptyOption + options.join('');
    };

    const handleTableChange = (event) => {
        const target = event.target;
        if (!target.dataset.field) return;
        const rowKey = parseInt(target.closest('tr')?.dataset?.rowKey, 10);
        const row = state.rows.find(r => r.rowKey === rowKey);
        if (!row) return;

        const field = target.dataset.field;
        let value = target.value;

        if (field === 'U_MGS_CL_IMPORT') {
            const cleaned = parseAmount(value);
            row[field] = cleaned;
            if (isNaN(cleaned)) {
                target.classList.add('celda-invalida');
            } else {
                target.classList.remove('celda-invalida');
            }
        } else if (field === 'U_MGS_CL_FECHA') {
            row[field] = value;
            if (!isFechaValida(value)) {
                target.classList.add('celda-invalida');
            } else {
                target.classList.remove('celda-invalida');
            }
        } else {
            row[field] = value;
        }

        markPending(rowKey);
    };

    const handleTableBlur = (event) => {
        const target = event.target;
        if (target.dataset?.field === 'U_MGS_CL_IMPORT') {
            const value = parseAmount(target.value);
            if (!isNaN(value)) {
                target.value = formatAmount(value);
                target.classList.remove('celda-invalida');
            }
        }
    };

    const handleItemSearch = debounce(async (event) => {
        const target = event.target;
        if (!target.classList.contains('input-item')) return;
        const query = target.value.trim();
        if (query.length < 2) return;

        try {
            const items = await fetchJson(`/PrevisionGastos/Items?search=${encodeURIComponent(query)}`);
            state.itemsCache = items || [];
            renderItemsList();
        } catch (error) {
            showAlert('warning', 'No se pudo cargar la ayuda de artículos.');
        }
    }, 300);

    const renderItemsList = () => {
        elements.listaItems.innerHTML = '';
        state.itemsCache.forEach(item => {
            const option = document.createElement('option');
            option.value = item.ItemCode;
            option.textContent = `${item.ItemCode} - ${item.ItemName}`;
            elements.listaItems.appendChild(option);
        });
    };

    const guardarTodo = async () => {
        if (!navigator.onLine) {
            showAlert('warning', 'No tiene conexión. Vuelva a intentar.');
            return;
        }

        clearAlerts();
        const periodo = elements.periodo.value;
        if (!periodo) {
            showAlert('danger', 'Debe seleccionar un periodo.');
            return;
        }

        if (!validarAntesGuardar()) {
            showAlert('danger', 'Existen campos inválidos. Corrija antes de actualizar.');
            return;
        }

        const payload = {
            DocEntry: state.docEntry || '',
            U_MGS_CL_PERIODO: periodo,
            MGS_CL_GASDETCollection: state.rows.map(row => ({
                LineId: row.LineId ? parseInt(row.LineId, 10) : null,
                U_MGS_CL_TIENDA: row.U_MGS_CL_TIENDA || '',
                U_MGS_CL_CONPRM: row.U_MGS_CL_CONPRM || '',
                U_MGS_CL_TIPMOP: row.U_MGS_CL_TIPMOP || '',
                U_MGS_CL_ITEMCOD: row.U_MGS_CL_ITEMCOD || '',
                U_MGS_CL_FECHA: row.U_MGS_CL_FECHA || '',
                U_MGS_CL_IMPORT: isNaN(Number(row.U_MGS_CL_IMPORT)) ? null : Number(row.U_MGS_CL_IMPORT),
                U_MGS_CL_VALIDO: row.U_MGS_CL_VALIDO || 'NO'
            }))
        };

        setLoading(true, 'Actualizando...');
        try {
            const resp = await fetchJson('/PrevisionGastos/ActualizarTodo', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (resp?.registered === false || resp?.Registered === false) {
                showAlert('danger', resp?.message || resp?.Message || 'No se pudo guardar la información.');
                return;
            }

            showAlert('success', 'Información actualizada correctamente.');
            await buscar();
        } catch (error) {
            showAlert('danger', 'No se pudo guardar la información.');
        } finally {
            setLoading(false);
        }
    };

    const validarAntesGuardar = () => {
        let valido = true;
        const periodo = elements.periodo.value;
        state.rows.forEach(row => {
            if (!isFechaValida(row.U_MGS_CL_FECHA, periodo)) {
                valido = false;
            }
            if (row.U_MGS_CL_IMPORT === null || row.U_MGS_CL_IMPORT === undefined || isNaN(row.U_MGS_CL_IMPORT)) {
                valido = false;
            }
        });

        elements.tablaBody.querySelectorAll('input[data-field="U_MGS_CL_FECHA"]').forEach(input => {
            const isOk = isFechaValida(input.value, periodo);
            input.classList.toggle('celda-invalida', !isOk);
        });

        elements.tablaBody.querySelectorAll('input[data-field="U_MGS_CL_IMPORT"]').forEach(input => {
            const value = parseAmount(input.value);
            const isOk = !isNaN(value);
            input.classList.toggle('celda-invalida', !isOk);
        });

        return valido;
    };

    const markPending = (rowKey) => {
        state.pending.add(rowKey);
        updatePendingBadge();
        const row = elements.tablaBody.querySelector(`tr[data-row-key="${rowKey}"]`);
        if (row) {
            row.classList.add('fila-pendiente');
        }
    };

    const clearPending = () => {
        state.pending.clear();
        updatePendingBadge();
        elements.tablaBody.querySelectorAll('tr').forEach(tr => tr.classList.remove('fila-pendiente'));
    };

    const updatePendingBadge = () => {
        elements.pendingBadge.textContent = state.pending.size.toString();
    };

    const formatAmount = (value) => {
        const number = Number(value);
        if (isNaN(number)) return '';
        return number.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    };

    const parseAmount = (value) => {
        if (value === null || value === undefined) return NaN;
        const cleaned = value.toString().replace(/,/g, '');
        const number = Number(cleaned);
        return isNaN(number) ? NaN : number;
    };

    const defaultFechaPeriodo = (periodo) => {
        const [year, month] = periodo.split('-').map(Number);
        const inicio = new Date(year, month - 1, 1);
        const fin = new Date(year, month, 0);
        const hoy = new Date();
        if (hoy >= inicio && hoy <= fin) {
            return hoy.toISOString().split('T')[0];
        }
        return inicio.toISOString().split('T')[0];
    };

    const isFechaValida = (fecha, periodo = elements.periodo.value) => {
        if (!fecha || !periodo) return false;
        const [year, month] = periodo.split('-').map(Number);
        const inicio = new Date(year, month - 1, 1);
        const fin = new Date(year, month, 0);
        const fechaObj = new Date(`${fecha}T00:00:00`);
        return fechaObj >= inicio && fechaObj <= fin;
    };

    const cambiarPeriodo = (increment) => {
        const current = elements.periodo.value;
        if (!current) return;
        const [year, month] = current.split('-').map(Number);
        const date = new Date(year, month - 1 + increment, 1);
        const newValue = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;

        if (state.pending.size > 0) {
            requestDiscard(() => {
                elements.periodo.value = newValue;
                state.lastPeriod = newValue;
            }, () => {
                elements.periodo.value = state.lastPeriod;
            });
            return;
        }

        elements.periodo.value = newValue;
        state.lastPeriod = newValue;
    };

    const fetchJson = async (url, options = {}) => {
        const response = await fetch(url, options);
        if (!response.ok) {
            throw new Error('Error en la solicitud');
        }
        return await response.json();
    };

    const setLoading = (show, message = 'Procesando...') => {
        if (show) {
            elements.loadingMessage.textContent = message;
            elements.loadingOverlay.classList.remove('d-none');
        } else {
            elements.loadingOverlay.classList.add('d-none');
        }
    };

    const showAlert = (type, message) => {
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.role = 'alert';
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
        elements.alertContainer.appendChild(alert);
    };

    const clearAlerts = () => {
        elements.alertContainer.innerHTML = '';
    };

    const escapeHtml = (value) => {
        return value
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    };

    const debounce = (fn, delay) => {
        let timer;
        return (...args) => {
            clearTimeout(timer);
            timer = setTimeout(() => fn(...args), delay);
        };
    };

    init();
})();
