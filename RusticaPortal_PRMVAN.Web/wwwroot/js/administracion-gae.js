document.addEventListener('DOMContentLoaded', () => {
    const api = {
        tiendas: '/AdministracionGAE/Tiendas',
        tiposGae: '/AdministracionGAE/TiposGae',
        tiposGasto: '/AdministracionGAE/TiposGasto',
        motivos: '/AdministracionGAE/MotivosGasto',
        buscar: '/AdministracionGAE/Buscar',
        actualizar: '/AdministracionGAE/ActualizarTodo'
    };

    const state = {
        tiendas: [],
        tiposGae: [],
        tiposGasto: [],
        motivos: [],
        rows: [],
        originalRows: [],
        pendientes: 0,
        pendingAction: null,
        page: 1,
        pageSize: 50,
        thereAreMoreRows: false
    };

    const elements = {
        filtroPeriodo: document.getElementById('filtroPeriodo'),
        filtroDesde: document.getElementById('filtroDesde'),
        filtroHasta: document.getElementById('filtroHasta'),
        filtroFactura: document.getElementById('filtroFactura'),
        filtroConcepto: document.getElementById('filtroConcepto'),
        filtroTipoGasto: document.getElementById('filtroTipoGasto'),
        filtroMotivo: document.getElementById('filtroMotivo'),
        tiendasEstado: document.getElementById('tiendasEstado'),
        tiendasListado: document.getElementById('tiendasListado'),
        tiendasDropdownBtn: document.getElementById('tiendasDropdownBtn'),
        tiposGaeEstado: document.getElementById('tiposGaeEstado'),
        tiposGaeListado: document.getElementById('tiposGaeListado'),
        tipoGaeDropdownBtn: document.getElementById('tipoGaeDropdownBtn'),
        pageSize: document.getElementById('pageSize'),
        pageInfo: document.getElementById('pageInfo'),
        btnPrevPage: document.getElementById('btnPrevPage'),
        btnNextPage: document.getElementById('btnNextPage'),
        btnBuscar: document.getElementById('btnBuscar'),
        btnActualizarTodo: document.getElementById('btnActualizarTodo'),
        btnExportar: document.getElementById('btnExportar'),
        pendingBadge: document.getElementById('pendingBadge'),
        tablaBody: document.querySelector('#tablaGae tbody'),
        alertContainer: document.getElementById('alertContainer'),
        loadingOverlay: document.getElementById('loadingOverlay'),
        loadingMessage: document.getElementById('loadingMessage'),
        modalCambios: document.getElementById('modalCambios'),
        btnDescartar: document.getElementById('btnDescartar'),
        colEstadoHeader: document.querySelector('#tablaGae thead .col-estado'),
        colErrorHeader: document.querySelector('#tablaGae thead .col-error'),
        tableScroll: document.getElementById('gaeTableScroll'),
        stickyScroll: document.getElementById('gaeStickyScrollbar'),
        stickyScrollInner: document.getElementById('gaeStickyScrollbarInner')
    };

    const connectionMessage = 'No tiene conexión. Intente nuevamente.';

    const getValue = (obj, keys) => {
        for (const key of keys) {
            if (obj && Object.prototype.hasOwnProperty.call(obj, key) && obj[key] != null) {
                return obj[key];
            }
        }
        return '';
    };


    const normalizeYnValue = (value) => {
        const normalized = String(value ?? '').trim().toUpperCase();
        if (normalized === 'Y' || normalized === 'SI') return 'Y';
        if (normalized === 'N' || normalized === 'NO') return 'N';
        return '';
    };

    const normalizeOption = (item) => ({
        Code: String(getValue(item, ['Code', 'code', 'Codigo', 'codigo', 'Value', 'value'])).trim(),
        Name: String(getValue(item, ['Name', 'name', 'Nombre', 'nombre', 'Label', 'label'])).trim()
    });

    const normalizeOptions = (items) => (items || [])
        .map(normalizeOption)
        .filter((item) => item.Code && item.Name);

    const setLoading = (show, message = 'Procesando...') => {
        if (show) {
            elements.loadingMessage.textContent = message;
            elements.loadingOverlay.classList.remove('d-none');
        } else {
            elements.loadingOverlay.classList.add('d-none');
        }
    };

    const showAlert = (message, type = 'info') => {
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.innerHTML = `${message}<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>`;
        elements.alertContainer.appendChild(alert);
    };

    const clearAlerts = () => {
        elements.alertContainer.innerHTML = '';
    };

    const fetchJson = async (url, options = {}) => {
        if (!navigator.onLine) {
            throw new Error(connectionMessage);
        }
        const response = await fetch(url, options);
        const contentType = response.headers.get('content-type') || '';
        if (!response.ok) {
            const payload = contentType.includes('application/json') ? await response.json() : await response.text();
            const message = typeof payload === 'string' ? payload : (payload.message || JSON.stringify(payload));
            throw new Error(message);
        }
        return contentType.includes('application/json') ? response.json() : [];
    };

    const updatePendingBadge = () => {
        elements.pendingBadge.textContent = String(state.pendientes);
    };

    const getSelectedValues = (container) => Array.from(container.querySelectorAll('input[type="checkbox"]'))
        .filter((input) => input.checked)
        .map((input) => input.value);

    const getSelectedTiendas = () => getSelectedValues(elements.tiendasListado);
    const getSelectedTiposGae = () => getSelectedValues(elements.tiposGaeListado);

    const setAllChecks = (container, checked) => {
        container.querySelectorAll('input[type="checkbox"]').forEach((input) => {
            input.checked = checked;
        });
    };

    const updateMultiLabel = (button, selected, total) => {
        if (!total || selected === total) {
            button.textContent = 'Todas';
            return;
        }
        if (!selected) {
            button.textContent = 'Sin selección';
            return;
        }
        button.textContent = `${selected} seleccionadas`;
    };

    const renderChecklist = (container, stateLabel, items, prefix) => {
        container.innerHTML = '';
        if (!items.length) {
            stateLabel.textContent = 'No hay datos disponibles.';
            return;
        }
        stateLabel.textContent = `${items.length} registros cargados`;
        items.forEach((item, index) => {
            const id = `${prefix}-${index}`;
            const wrapper = document.createElement('div');
            wrapper.className = 'form-check';
            wrapper.innerHTML = `
                <input class="form-check-input" type="checkbox" value="${item.Code}" id="${id}" checked>
                <label class="form-check-label" for="${id}">${item.Code} - ${item.Name}</label>
            `;
            container.appendChild(wrapper);
        });
    };

    const renderSelectOptions = (select, items, allLabel = 'TODOS') => {
        select.innerHTML = '';
        const allOption = document.createElement('option');
        allOption.value = '';
        allOption.textContent = allLabel;
        select.appendChild(allOption);
        items.forEach((item) => {
            const option = document.createElement('option');
            option.value = item.Code;
            option.textContent = item.Name;
            select.appendChild(option);
        });
    };

    const periodToRange = (period) => {
        if (!period || !/^\d{4}-\d{2}$/.test(period)) return null;
        const [year, month] = period.split('-').map(Number);
        const first = new Date(year, month - 1, 1);
        const last = new Date(year, month, 0);
        const toIso = (d) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
        return { from: toIso(first), to: toIso(last) };
    };

    const validatePeriodoRange = () => {
        const range = periodToRange(elements.filtroPeriodo.value);
        if (!range) {
            showAlert('Seleccione un periodo válido (YYYY-MM).', 'warning');
            return false;
        }
        const desde = elements.filtroDesde.value;
        const hasta = elements.filtroHasta.value;
        if (!desde || !hasta) {
            showAlert('Debe ingresar fechas Desde y Hasta.', 'warning');
            return false;
        }
        if (desde < range.from || hasta > range.to) {
            showAlert('Las fechas Desde/Hasta deben estar dentro del periodo seleccionado.', 'warning');
            return false;
        }
        if (desde > hasta) {
            showAlert('La fecha Desde no puede ser mayor a Hasta.', 'warning');
            return false;
        }
        return true;
    };

    const setPendiente = (row) => {
        if (!row.pendiente) {
            row.pendiente = true;
            state.pendientes += 1;
            updatePendingBadge();
        }
        row.estado = 'Pendiente';
    };

    const clearPendiente = (row) => {
        if (row.pendiente) {
            row.pendiente = false;
            state.pendientes = Math.max(0, state.pendientes - 1);
            updatePendingBadge();
        }
    };

    const setErrorColumnsVisible = (visible) => {
        elements.colEstadoHeader.classList.toggle('d-none', !visible);
        elements.colErrorHeader.classList.toggle('d-none', !visible);
        document.querySelectorAll('#tablaGae tbody .col-estado, #tablaGae tbody .col-error').forEach((cell) => {
            cell.classList.toggle('d-none', !visible);
        });
    };

    const updateRowStatus = (rowEl, row) => {
        if (!rowEl) return;
        rowEl.classList.toggle('fila-pendiente', row.pendiente);
        rowEl.classList.toggle('fila-error', row.estado === 'Error' || Boolean(row.mensajeError));
        const estadoEl = rowEl.querySelector('.estado');
        if (estadoEl) {
            estadoEl.textContent = row.estado || '';
            estadoEl.classList.remove('estado-ok', 'estado-error', 'estado-pendiente');
            if (row.estado === 'OK') estadoEl.classList.add('estado-ok');
            if (row.estado === 'Error') estadoEl.classList.add('estado-error');
            if (row.estado === 'Pendiente') estadoEl.classList.add('estado-pendiente');
        }
        const errorCell = rowEl.querySelector('.col-error');
        if (errorCell) {
            errorCell.textContent = row.mensajeError || '';
        }
    };

    const renderSelectInline = (field, value, items) => {
        const options = items.map((item) => `<option value="${item.Code}" ${item.Code === value ? 'selected' : ''}>${item.Name}</option>`).join('');
        return `<select class="form-select form-select-sm select-inline" data-field="${field}"><option value="">Seleccionar</option>${options}</select>`;
    };

    const renderRows = () => {
        elements.tablaBody.innerHTML = '';
        state.rows.forEach((row, index) => {
            const tr = document.createElement('tr');
            tr.dataset.index = String(index);
            tr.innerHTML = `
                <td><input type="checkbox" class="form-check-input" data-field="seleccionado" ${row.seleccionado ? 'checked' : ''}></td>
                <td>${row.docEntry}</td>
                <td>${row.lineId}</td>
                <td>${row.fecFiltro || ''}</td>
                <td>${row.numAtCard}</td>
                <td>${row.concepto}</td>
                <td>${row.tienda}</td>
                <td>${renderSelectInline('U_MGS_CL_TIPGAE', row.U_MGS_CL_TIPGAE, state.tiposGae)}</td>
                <td><input type="checkbox" class="form-check-input" data-field="U_MGS_CL_AUTORI" ${row.U_MGS_CL_AUTORI === 'Y' ? 'checked' : ''}></td>
                <td>${renderSelectInline('U_MGS_CL_TIPGAS', row.U_MGS_CL_TIPGAS, state.tiposGasto)}</td>
                <td>${renderSelectInline('U_MGS_CL_TIPMOP', row.U_MGS_CL_TIPMOP, state.motivos)}</td>
                <td><input type="number" step="0.01" class="form-control form-control-sm input-inline input-importe" data-field="U_MGS_CL_IMPORT" value="${row.U_MGS_CL_IMPORT}"></td>
                <td><input type="date" class="form-control form-control-sm input-inline" data-field="U_MGS_CL_FEPRM" value="${row.U_MGS_CL_FEPRM}"></td>
                <td>${row.U_MGS_CL_SOLICI}</td>
                <td><input type="checkbox" class="form-check-input" data-field="U_MGS_CL_VALIDO" ${row.U_MGS_CL_VALIDO === 'Y' ? 'checked' : ''}></td>
                <td class="col-estado d-none"><span class="estado"></span></td>
                <td class="col-error d-none"></td>
            `;
            elements.tablaBody.appendChild(tr);
            updateRowStatus(tr, row);
        });

        const hasErrors = state.rows.some((row) => row.mensajeError);
        setErrorColumnsVisible(hasErrors);
    };

    const mapRow = (item) => ({
        idEmpresa: String(getValue(item, ['IdEmpresa', 'idEmpresa'])),
        nombreEmpresa: String(getValue(item, ['NombreEmpresa', 'nombreEmpresa'])),
        baseDatos: String(getValue(item, ['BaseDatos', 'baseDatos', 'NombreEmpresa', 'nombreEmpresa'])),
        objectType: String(getValue(item, ['ObjectType', 'objectType'])),
        docEntry: String(getValue(item, ['DocEntry', 'docEntry'])),
        lineId: String(getValue(item, ['LineId', 'lineId', 'LineNum', 'lineNum'])),
        fecFiltro: String(getValue(item, ['FecFiltro', 'fecFiltro'])),
        numAtCard: String(getValue(item, ['NumAtCard', 'numAtCard'])),
        concepto: String(getValue(item, ['Concepto', 'concepto'])),
        tienda: String(getValue(item, ['Tienda', 'tienda'])),
        U_MGS_CL_TIPGAE: String(getValue(item, ['U_MGS_CL_TIPGAE', 'u_MGS_CL_TIPGAE'])),
        U_MGS_CL_AUTORI: normalizeYnValue(getValue(item, ['U_MGS_CL_AUTORI', 'u_MGS_CL_AUTORI'])),
        U_MGS_CL_TIPGAS: String(getValue(item, ['U_MGS_CL_TIPGAS', 'u_MGS_CL_TIPGAS'])),
        U_MGS_CL_TIPMOP: String(getValue(item, ['U_MGS_CL_TIPMOP', 'u_MGS_CL_TIPMOP'])),
        U_MGS_CL_IMPORT: Number(getValue(item, ['U_MGS_CL_IMPORT', 'u_MGS_CL_IMPORT', 'importe']) || 0),
        U_MGS_CL_FEPRM: String(getValue(item, ['U_MGS_CL_FEPRM', 'u_MGS_CL_FEPRM'])).split('T')[0],
        U_MGS_CL_SOLICI: String(getValue(item, ['U_MGS_CL_SOLICI', 'u_MGS_CL_SOLICI'])),
        U_MGS_CL_VALIDO: normalizeYnValue(getValue(item, ['U_MGS_CL_VALIDO', 'u_MGS_CL_VALIDO'])),
        pendiente: String(getValue(item, ['Pendiente', 'pendiente'])) === 'SI',
        mensajeError: String(getValue(item, ['MensajeError', 'Message', 'message'])),
        estado: '',
        seleccionado: false
    });

    const recalcPending = () => {
        state.pendientes = state.rows.filter((row) => row.pendiente).length;
        updatePendingBadge();
    };

    const discardChanges = () => {
        state.rows = state.originalRows.map((row) => ({ ...row, seleccionado: false }));
        recalcPending();
        renderRows();
    };

    const confirmDiscard = (onConfirm, onCancel) => {
        if (!state.pendientes) {
            onConfirm?.();
            return;
        }
        state.pendingAction = { onConfirm, onCancel };
        bootstrap.Modal.getOrCreateInstance(elements.modalCambios).show();
    };

    const handlePendingResult = (confirm) => {
        const action = state.pendingAction;
        state.pendingAction = null;
        if (!action) return;
        if (confirm) {
            discardChanges();
            action.onConfirm?.();
        } else {
            action.onCancel?.();
        }
    };

    const updatePageInfo = () => {
        elements.pageInfo.textContent = `Página ${state.page} · ${state.rows.length} fila(s)`;
        elements.btnPrevPage.disabled = state.page <= 1;
        elements.btnNextPage.disabled = !state.thereAreMoreRows;
    };

    const buildFiltroPayload = () => {
        const factura = elements.filtroFactura.value.trim();
        const concepto = elements.filtroConcepto.value.trim();
        const selectedTiposGae = getSelectedTiposGae();
        const tipoGae = selectedTiposGae.length === state.tiposGae.length ? '' : selectedTiposGae.join(',');
        const tipoGasto = elements.filtroTipoGasto.value || '';
        const motivo = elements.filtroMotivo.value || '';
        const pagina = String(state.page || 1);
        const cantidad = String(state.pageSize || 50);
        return [factura, concepto, tipoGae, tipoGasto, motivo, pagina, cantidad].join('|');
    };

    const buscar = async () => {
        clearAlerts();
        if (!validatePeriodoRange()) return;

        setLoading(true, 'Buscando...');
        try {
            const query = new URLSearchParams(window.location.search);
            const empresa = query.get('empresa') || query.get('Empresa') || '1';
            const desde = elements.filtroDesde.value;
            const hasta = elements.filtroHasta.value;

            const selectedTiendas = getSelectedTiendas();
            const tiendas = selectedTiendas.length === state.tiendas.length ? '' : selectedTiendas.join(',');

            const factura = elements.filtroFactura.value.trim();
            const concepto = elements.filtroConcepto.value.trim();
            const selectedTiposGae = getSelectedTiposGae();
            const tipoGae = selectedTiposGae.length === state.tiposGae.length ? '' : selectedTiposGae.join(',');
            const tipoGas = elements.filtroTipoGasto.value || '';
            const motivo = elements.filtroMotivo.value || '';
            const pagina = String(state.page || 1);
            const cantidad = String(state.pageSize || 50);

            const filtrosRaw = `${factura}|${concepto}|${tipoGae}|${tipoGas}|${motivo}|${pagina}|${cantidad}`;
            const urlFinal = `${api.buscar}?empresa=${encodeURIComponent(empresa)}&fechaDesde=${encodeURIComponent(desde)}&fechaHasta=${encodeURIComponent(hasta)}&tiendas=${encodeURIComponent(tiendas)}&filtros=${encodeURIComponent(filtrosRaw)}`;

            console.log('GAE buscar filtrosRaw:', filtrosRaw);
            console.log('GAE buscar urlFinal:', urlFinal);

            const response = await fetch(urlFinal, { method: 'GET' });

            if (!response.ok) {
                const bodyText = await response.text();
                let mensaje = bodyText;
                try {
                    const parsed = JSON.parse(bodyText);
                    mensaje = parsed?.message || parsed?.Message || bodyText;
                } catch (_) {
                    mensaje = bodyText;
                }

                const errorMessage = `HTTP ${response.status} ${response.statusText} - ${mensaje || 'Sin detalle.'}`;
                console.error('GAE buscar error:', {
                    status: response.status,
                    statusText: response.statusText,
                    url: urlFinal,
                    bodyText
                });
                showAlert(errorMessage, 'danger');
                return;
            }

            const data = await response.json();
            state.rows = (data || []).map(mapRow).map((row) => {
                row.estado = row.pendiente ? 'Pendiente' : (row.mensajeError ? 'Error' : 'OK');
                return row;
            });
            state.originalRows = state.rows.map((row) => ({ ...row }));
            state.thereAreMoreRows = state.rows.length >= state.pageSize;
            recalcPending();
            renderRows();
            updatePageInfo();
        } catch (error) {
            console.error(error);
            showAlert('No tiene conexión. Intente nuevamente.', 'danger');
        } finally {
            setLoading(false);
        }
    };

    const actualizarTodo = async () => {
        clearAlerts();
        if (!navigator.onLine) {
            showAlert(connectionMessage, 'warning');
            return;
        }

        const selected = state.rows.filter((row) => row.seleccionado && row.pendiente);
        const pendientes = selected.length ? selected : state.rows.filter((row) => row.pendiente);

        if (!pendientes.length) {
            showAlert('No hay cambios pendientes.', 'info');
            return;
        }

        setLoading(true, 'Actualizando...');
        try {
            const payload = {
                items: pendientes.map((row) => ({
                    idEmpresa: row.idEmpresa,
                    nombreEmpresa: row.nombreEmpresa,
                    baseDatos: row.baseDatos,
                    objectType: row.objectType,
                    docEntry: row.docEntry,
                    lineId: row.lineId,
                    docNum: row.docNum || '',
                    numAtCard: row.numAtCard || '',
                    U_MGS_CL_TIPGAE: row.U_MGS_CL_TIPGAE,
                    U_MGS_CL_AUTORI: normalizeYnValue(row.U_MGS_CL_AUTORI),
                    U_MGS_CL_TIPGAS: row.U_MGS_CL_TIPGAS,
                    U_MGS_CL_TIPMOP: row.U_MGS_CL_TIPMOP,
                    U_MGS_CL_IMPORT: row.U_MGS_CL_IMPORT,
                    U_MGS_CL_FEPRM: row.U_MGS_CL_FEPRM,
                    U_MGS_CL_SOLICI: row.U_MGS_CL_SOLICI,
                    U_MGS_CL_VALIDO: normalizeYnValue(row.U_MGS_CL_VALIDO)
                }))
            };

            const response = await fetchJson(api.actualizar, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            const registered = Boolean(getValue(response, ['registered', 'Registered']));
            const message = String(getValue(response, ['message', 'Message'])).trim();
            const content = String(getValue(response, ['content', 'Content'])).trim();
            const items = Array.isArray(getValue(response, ['items', 'Items'])) ? getValue(response, ['items', 'Items']) : [];

            if (!registered) {
                if (!items.length) {
                    showAlert(message || content || 'Error al actualizar', 'danger');
                    return;
                }
            }

            const indexByKey = new Map();
            state.rows.forEach((row, idx) => {
                const fullKey = [row.baseDatos || '', row.objectType, row.docEntry, row.lineId].join('|');
                const shortKey = [row.objectType, row.docEntry, row.lineId].join('|');
                indexByKey.set(fullKey, idx);
                if (!indexByKey.has(shortKey)) indexByKey.set(shortKey, idx);
            });

            let errors = 0;
            items.forEach((result) => {
                const resultBase = String(getValue(result, ['BaseDatos', 'baseDatos']));
                const resultObjectType = String(getValue(result, ['ObjectType', 'objectType']));
                const resultDocEntry = String(getValue(result, ['DocEntry', 'docEntry']));
                const resultLineId = String(getValue(result, ['LineId', 'lineId']));

                const idx = indexByKey.get([resultBase, resultObjectType, resultDocEntry, resultLineId].join('|'))
                    ?? indexByKey.get([resultObjectType, resultDocEntry, resultLineId].join('|'));

                if (idx == null) return;
                const row = state.rows[idx];
                const ok = Boolean(getValue(result, ['Ok', 'ok']));

                if (ok) {
                    clearPendiente(row);
                    row.estado = 'OK';
                    row.mensajeError = '';
                } else {
                    errors += 1;
                    row.estado = 'Error';
                    row.mensajeError = String(getValue(result, ['Message', 'message'])) || 'Error al actualizar.';
                    setPendiente(row);
                }
                updateRowStatus(elements.tablaBody.querySelector(`tr[data-index="${idx}"]`), row);
            });

            if (!registered && errors === 0 && !items.length) {
                showAlert(message || content || 'Error al actualizar', 'danger');
                return;
            }

            if (errors > 0) {
                setErrorColumnsVisible(true);
                showAlert('Se encontraron errores al actualizar. Revise Estado/Mensaje.', 'danger');
                return;
            }

            if (registered && items.length === 0) {
                setErrorColumnsVisible(false);
                state.rows.forEach((row) => {
                    row.estado = row.pendiente ? 'Pendiente' : 'OK';
                    if (!row.pendiente) row.mensajeError = '';
                });
                renderRows();
                showAlert('Actualización realizada correctamente', 'success');
                state.page = 1;
                await buscar();
                return;
            }

            setErrorColumnsVisible(false);
            showAlert('Actualización realizada correctamente', 'success');
            state.page = 1;
            await buscar();
        } catch (error) {
            showAlert(error.message || connectionMessage, 'danger');
        } finally {
            setLoading(false);
        }
    };

    const exportar = () => {
        if (!state.rows.length) {
            showAlert('No hay datos para exportar.', 'warning');
            return;
        }
        const headers = ['Código interno', 'Línea', 'Fecha', 'N° Factura', 'Concepto', 'Tienda', 'GAE', 'Autorizado', 'Tipo de gasto', 'Motivo de gasto', 'Importe', 'Fecha PRM', 'Solicitado por', 'No válido', 'Estado', 'Mensaje'];
        const rows = state.rows.map((row) => [row.docEntry, row.lineId, row.fecFiltro, row.numAtCard, row.concepto, row.tienda, row.U_MGS_CL_TIPGAE, row.U_MGS_CL_AUTORI, row.U_MGS_CL_TIPGAS, row.U_MGS_CL_TIPMOP, row.U_MGS_CL_IMPORT, row.U_MGS_CL_FEPRM, row.U_MGS_CL_SOLICI, row.U_MGS_CL_VALIDO, row.estado, row.mensajeError]);
        const csv = [headers, ...rows].map((r) => r.map((v) => `"${String(v ?? '').replace(/"/g, '""')}"`).join(',')).join('\n');
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'administracion-gae.csv';
        a.click();
        URL.revokeObjectURL(url);
    };


    const setupHorizontalScrollSync = () => {
        if (!elements.tableScroll || !elements.stickyScroll || !elements.stickyScrollInner) return;

        let syncing = false;
        const syncSize = () => {
            const table = document.getElementById('tablaGae');
            if (!table) return;
            elements.stickyScrollInner.style.width = `${table.scrollWidth}px`;
            elements.stickyScroll.classList.toggle('d-none', table.scrollWidth <= elements.tableScroll.clientWidth);
        };

        elements.stickyScroll.addEventListener('scroll', () => {
            if (syncing) return;
            syncing = true;
            elements.tableScroll.scrollLeft = elements.stickyScroll.scrollLeft;
            syncing = false;
        });

        elements.tableScroll.addEventListener('scroll', () => {
            if (syncing) return;
            syncing = true;
            elements.stickyScroll.scrollLeft = elements.tableScroll.scrollLeft;
            syncing = false;
        });

        window.addEventListener('resize', syncSize);
        const observer = new MutationObserver(syncSize);
        observer.observe(elements.tablaBody, { childList: true, subtree: true });
        syncSize();
    };

    const loadCatalogos = async () => {
        setLoading(true, 'Cargando catálogos...');
        try {
            const [tiendas, tiposGae, tiposGasto, motivos] = await Promise.all([
                fetchJson(api.tiendas),
                fetchJson(api.tiposGae),
                fetchJson(api.tiposGasto),
                fetchJson(api.motivos)
            ]);

            state.tiendas = normalizeOptions(tiendas);
            state.tiposGae = normalizeOptions(tiposGae);
            state.tiposGasto = normalizeOptions(tiposGasto);
            state.motivos = normalizeOptions(motivos);

            renderChecklist(elements.tiendasListado, elements.tiendasEstado, state.tiendas, 'tienda');
            renderChecklist(elements.tiposGaeListado, elements.tiposGaeEstado, state.tiposGae, 'tipogae');
            renderSelectOptions(elements.filtroTipoGasto, state.tiposGasto);
            renderSelectOptions(elements.filtroMotivo, state.motivos);

            updateMultiLabel(elements.tiendasDropdownBtn, getSelectedTiendas().length, state.tiendas.length);
            updateMultiLabel(elements.tipoGaeDropdownBtn, getSelectedTiposGae().length, state.tiposGae.length);
        } catch (error) {
            showAlert(error.message || connectionMessage, 'warning');
        } finally {
            setLoading(false);
        }
    };

    const tryFilterAction = (onConfirm, onCancel) => confirmDiscard(onConfirm, onCancel);

    setupHorizontalScrollSync();

    elements.btnDescartar.addEventListener('click', () => {
        handlePendingResult(true);
        bootstrap.Modal.getOrCreateInstance(elements.modalCambios).hide();
    });

    elements.modalCambios.addEventListener('hidden.bs.modal', () => {
        if (state.pendingAction) {
            handlePendingResult(false);
        }
    });

    elements.filtroPeriodo.addEventListener('change', () => {
        tryFilterAction(() => {
            const range = periodToRange(elements.filtroPeriodo.value);
            if (range) {
                elements.filtroDesde.value = range.from;
                elements.filtroHasta.value = range.to;
            }
        }, () => {
            const fallback = periodToRange(elements.filtroPeriodo.defaultValue);
            if (fallback) {
                elements.filtroPeriodo.value = elements.filtroPeriodo.defaultValue;
                elements.filtroDesde.value = fallback.from;
                elements.filtroHasta.value = fallback.to;
            }
        });
    });

    [elements.filtroDesde, elements.filtroHasta, elements.filtroFactura, elements.filtroConcepto, elements.filtroTipoGasto, elements.filtroMotivo].forEach((input) => {
        input.addEventListener('change', () => tryFilterAction(() => {}, () => {}));
    });

    elements.tiendasListado.addEventListener('change', (event) => {
        if (event.target.type !== 'checkbox') return;
        tryFilterAction(() => {
            updateMultiLabel(elements.tiendasDropdownBtn, getSelectedTiendas().length, state.tiendas.length);
        }, () => {
            event.target.checked = !event.target.checked;
        });
    });

    elements.tiposGaeListado.addEventListener('change', (event) => {
        if (event.target.type !== 'checkbox') return;
        tryFilterAction(() => {
            updateMultiLabel(elements.tipoGaeDropdownBtn, getSelectedTiposGae().length, state.tiposGae.length);
        }, () => {
            event.target.checked = !event.target.checked;
        });
    });

    document.getElementById('tiendaSelTodas').addEventListener('click', (event) => {
        event.preventDefault();
        tryFilterAction(() => {
            setAllChecks(elements.tiendasListado, true);
            updateMultiLabel(elements.tiendasDropdownBtn, getSelectedTiendas().length, state.tiendas.length);
        });
    });

    document.getElementById('tiendaLimpiar').addEventListener('click', (event) => {
        event.preventDefault();
        tryFilterAction(() => {
            setAllChecks(elements.tiendasListado, false);
            updateMultiLabel(elements.tiendasDropdownBtn, getSelectedTiendas().length, state.tiendas.length);
        });
    });

    document.getElementById('tipoGaeSelTodos').addEventListener('click', (event) => {
        event.preventDefault();
        tryFilterAction(() => {
            setAllChecks(elements.tiposGaeListado, true);
            updateMultiLabel(elements.tipoGaeDropdownBtn, getSelectedTiposGae().length, state.tiposGae.length);
        });
    });

    document.getElementById('tipoGaeLimpiar').addEventListener('click', (event) => {
        event.preventDefault();
        tryFilterAction(() => {
            setAllChecks(elements.tiposGaeListado, false);
            updateMultiLabel(elements.tipoGaeDropdownBtn, getSelectedTiposGae().length, state.tiposGae.length);
        });
    });

    elements.pageSize.addEventListener('change', () => {
        const previous = state.pageSize;
        const newSize = Number(elements.pageSize.value || 50);
        tryFilterAction(async () => {
            state.pageSize = newSize;
            state.page = 1;
            state.rows = [];
            renderRows();
            await buscar();
        }, () => {
            elements.pageSize.value = String(previous);
        });
    });

    elements.btnPrevPage.addEventListener('click', () => {
        if (state.page <= 1) return;
        tryFilterAction(async () => {
            state.page -= 1;
            state.rows = [];
            renderRows();
            await buscar();
        });
    });

    elements.btnNextPage.addEventListener('click', () => {
        tryFilterAction(async () => {
            state.page += 1;
            state.rows = [];
            renderRows();
            await buscar();
        });
    });

    elements.btnBuscar.addEventListener('click', () => {
        tryFilterAction(async () => {
            state.page = 1;
            await buscar();
        });
    });

    elements.btnActualizarTodo.addEventListener('click', () => {
        actualizarTodo();
    });

    elements.btnExportar.addEventListener('click', exportar);

    elements.tablaBody.addEventListener('change', (event) => {
        const target = event.target;
        const rowEl = target.closest('tr');
        if (!rowEl) return;
        const idx = Number(rowEl.dataset.index);
        const row = state.rows[idx];
        if (!row) return;

        const field = target.dataset.field;
        if (!field) return;
        if (field === 'seleccionado') {
            row.seleccionado = target.checked;
            return;
        }

        row[field] = target.type === 'checkbox'
            ? (target.checked ? 'Y' : 'N')
            : (field === 'U_MGS_CL_AUTORI' || field === 'U_MGS_CL_VALIDO' ? normalizeYnValue(target.value) : target.value);
        setPendiente(row);
        updateRowStatus(rowEl, row);

        const bulkFields = ['U_MGS_CL_AUTORI', 'U_MGS_CL_TIPGAS', 'U_MGS_CL_TIPMOP', 'U_MGS_CL_TIPGAE'];
        if (row.seleccionado && bulkFields.includes(field)) {
            state.rows.forEach((item, index) => {
                if (!item.seleccionado || index === idx) return;
                item[field] = row[field];
                setPendiente(item);
                const cellInput = elements.tablaBody.querySelector(`tr[data-index="${index}"] [data-field="${field}"]`);
                if (cellInput) {
                    if (cellInput.type === 'checkbox') {
                        cellInput.checked = item[field] === 'Y';
                    } else {
                        cellInput.value = item[field];
                    }
                }
                updateRowStatus(elements.tablaBody.querySelector(`tr[data-index="${index}"]`), item);
            });
        }
    });

    document.addEventListener('click', (event) => {
        const link = event.target.closest('a');
        if (!link || link.getAttribute('href')?.startsWith('#')) return;
        if (!state.pendientes) return;
        event.preventDefault();
        confirmDiscard(() => {
            window.location.href = link.href;
        });
    });

    window.addEventListener('beforeunload', (event) => {
        if (!state.pendientes) return;
        event.preventDefault();
        event.returnValue = 'Hay cambios pendientes. ¿Desea descartar los cambios?';
    });

    const initialRange = periodToRange(elements.filtroPeriodo.value);
    if (initialRange) {
        elements.filtroDesde.value = initialRange.from;
        elements.filtroHasta.value = initialRange.to;
        elements.filtroPeriodo.defaultValue = elements.filtroPeriodo.value;
    }
    state.pageSize = Number(elements.pageSize.value || 50);
    updatePendingBadge();
    updatePageInfo();
    loadCatalogos();
});