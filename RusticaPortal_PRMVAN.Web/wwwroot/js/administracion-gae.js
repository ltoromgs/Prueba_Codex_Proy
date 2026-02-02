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
        pendingNavigation: null,
        lastFilters: null
    };

    const elements = {
        filtroDesde: document.getElementById('filtroDesde'),
        filtroHasta: document.getElementById('filtroHasta'),
        filtroFactura: document.getElementById('filtroFactura'),
        filtroConcepto: document.getElementById('filtroConcepto'),
        filtroTipoGae: document.getElementById('filtroTipoGae'),
        filtroTipoGasto: document.getElementById('filtroTipoGasto'),
        filtroMotivo: document.getElementById('filtroMotivo'),
        tiendasEstado: document.getElementById('tiendasEstado'),
        tiendasListado: document.getElementById('tiendasListado'),
        tiendasDropdownBtn: document.getElementById('tiendasDropdownBtn'),
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
        colErrorHeader: document.querySelector('#tablaGae thead .col-error')
    };

    const connectionMessage = 'No tiene conexión. Intente nuevamente.';

    const getValue = (obj, keys) => {
        for (const key of keys) {
            if (obj && Object.prototype.hasOwnProperty.call(obj, key)) {
                return obj[key];
            }
        }
        return '';
    };

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
        alert.role = 'alert';
        alert.innerHTML = `${message}<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>`;
        elements.alertContainer.appendChild(alert);
    };

    const clearAlerts = () => {
        elements.alertContainer.innerHTML = '';
    };

    const fetchJson = async (url, options = {}) => {
        const response = await fetch(url, options);
        if (!response.ok) {
            const text = await response.text();
            throw new Error(text || 'Error en la solicitud');
        }
        return await response.json();
    };

    const updatePendingBadge = () => {
        elements.pendingBadge.textContent = state.pendientes.toString();
    };

    const setPendiente = (row) => {
        if (!row.pendiente) {
            row.pendiente = true;
            state.pendientes += 1;
            updatePendingBadge();
        }
        row.estado = 'Pendiente';
    };

    const ensurePendiente = (row) => {
        if (!row.pendiente) {
            row.pendiente = true;
            state.pendientes += 1;
            updatePendingBadge();
        }
    };

    const clearPendiente = (row) => {
        if (row.pendiente) {
            row.pendiente = false;
            state.pendientes = Math.max(0, state.pendientes - 1);
            updatePendingBadge();
        }
    };

    const setErrorColumnVisible = (visible) => {
        if (!elements.colErrorHeader) return;
        if (visible) {
            elements.colErrorHeader.classList.remove('d-none');
            document.querySelectorAll('#tablaGae tbody .col-error').forEach((cell) => {
                cell.classList.remove('d-none');
            });
        } else {
            elements.colErrorHeader.classList.add('d-none');
            document.querySelectorAll('#tablaGae tbody .col-error').forEach((cell) => {
                cell.classList.add('d-none');
            });
        }
    };

    const renderTiendas = () => {
        elements.tiendasListado.innerHTML = '';
        if (!state.tiendas.length) {
            elements.tiendasEstado.textContent = 'No hay tiendas disponibles.';
            return;
        }

        elements.tiendasEstado.textContent = `Tiendas cargadas: ${state.tiendas.length}`;
        state.tiendas.forEach((tienda, index) => {
            const code = getValue(tienda, ['Codigo', 'codigo', 'Code', 'code', 'PrjCode', 'prjCode']);
            const name = getValue(tienda, ['Nombre', 'nombre', 'Name', 'name', 'PrjName', 'prjName']);
            const id = `tienda-${index}`;
            const wrapper = document.createElement('div');
            wrapper.className = 'form-check';
            wrapper.innerHTML = `
                <input class="form-check-input" type="checkbox" value="${code}" id="${id}">
                <label class="form-check-label" for="${id}">${code} - ${name}</label>
            `;
            elements.tiendasListado.appendChild(wrapper);
        });
    };

    const updateTiendasLabel = () => {
        const selected = getSelectedTiendas();
        if (!selected.length) {
            elements.tiendasDropdownBtn.textContent = 'Seleccionar tiendas';
            return;
        }
        elements.tiendasDropdownBtn.textContent = `${selected.length} tienda(s) seleccionada(s)`;
    };

    const getSelectedTiendas = () => {
        return Array.from(elements.tiendasListado.querySelectorAll('input[type="checkbox"]'))
            .filter((input) => input.checked)
            .map((input) => input.value);
    };

    const renderSelectOptions = (select, items, placeholder) => {
        select.innerHTML = '';
        const optionAll = document.createElement('option');
        optionAll.value = '';
        optionAll.textContent = placeholder;
        select.appendChild(optionAll);

        items.forEach((item) => {
            const option = document.createElement('option');
            option.value = item.Code;
            option.textContent = item.Name;
            select.appendChild(option);
        });
    };

    const updateLastFilters = () => {
        state.lastFilters = {
            fechaDesde: elements.filtroDesde.value,
            fechaHasta: elements.filtroHasta.value,
            factura: elements.filtroFactura.value,
            concepto: elements.filtroConcepto.value,
            tipoGae: elements.filtroTipoGae.value,
            tipoGasto: elements.filtroTipoGasto.value,
            motivo: elements.filtroMotivo.value,
            tiendas: getSelectedTiendas()
        };
    };

    const restoreFilters = () => {
        if (!state.lastFilters) return;
        elements.filtroDesde.value = state.lastFilters.fechaDesde;
        elements.filtroHasta.value = state.lastFilters.fechaHasta;
        elements.filtroFactura.value = state.lastFilters.factura;
        elements.filtroConcepto.value = state.lastFilters.concepto;
        elements.filtroTipoGae.value = state.lastFilters.tipoGae;
        elements.filtroTipoGasto.value = state.lastFilters.tipoGasto;
        elements.filtroMotivo.value = state.lastFilters.motivo;
        const selected = new Set(state.lastFilters.tiendas);
        elements.tiendasListado.querySelectorAll('input[type="checkbox"]').forEach((input) => {
            input.checked = selected.has(input.value);
        });
        updateTiendasLabel();
    };

    const confirmDiscard = (onConfirm, onCancel) => {
        if (!state.pendientes) {
            onConfirm();
            return;
        }
        state.pendingAction = { onConfirm, onCancel };
        const modal = bootstrap.Modal.getOrCreateInstance(elements.modalCambios);
        modal.show();
    };

    const discardChanges = () => {
        if (!state.originalRows.length) {
            state.rows = [];
            state.pendientes = 0;
            updatePendingBadge();
            return;
        }
        state.rows = state.originalRows.map((row) => ({ ...row, seleccionado: false }));
        state.pendientes = state.rows.filter((row) => row.pendiente).length;
        updatePendingBadge();
        renderRows();
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

    elements.btnDescartar.addEventListener('click', () => {
        handlePendingResult(true);
        bootstrap.Modal.getOrCreateInstance(elements.modalCambios).hide();
    });

    elements.modalCambios.addEventListener('hidden.bs.modal', () => {
        if (state.pendingAction) {
            handlePendingResult(false);
        }
    });

    const applyBulkChange = (field, value) => {
        const selectedIndexes = state.rows
            .map((row, idx) => (row.seleccionado ? idx : null))
            .filter((idx) => idx !== null);
        selectedIndexes.forEach((idx) => {
            const row = state.rows[idx];
            row[field] = value;
            setPendiente(row);
            updateRowInputs(idx, field, value);
        });
    };

    const updateRowInputs = (index, field, value) => {
        const rowEl = elements.tablaBody.querySelector(`tr[data-index="${index}"]`);
        if (!rowEl) return;
        const input = rowEl.querySelector(`[data-field="${field}"]`);
        if (!input) return;
        if (input.type === 'checkbox') {
            input.checked = value === 'SI';
        } else {
            input.value = value ?? '';
        }
        updateRowStatus(rowEl, state.rows[index]);
    };

    const updateRowStatus = (rowEl, row) => {
        if (!rowEl) return;
        rowEl.classList.toggle('fila-pendiente', row.pendiente);
        const estadoEl = rowEl.querySelector('.estado');
        if (estadoEl) {
            estadoEl.textContent = row.estado;
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

    const renderRows = () => {
        elements.tablaBody.innerHTML = '';
        state.rows.forEach((row, index) => {
            const tr = document.createElement('tr');
            tr.dataset.index = index.toString();
            tr.dataset.baseDatos = row.baseDatos;
            tr.dataset.objectType = row.objectType;
            tr.dataset.docEntry = row.docEntry;
            tr.dataset.lineId = row.lineId;

            tr.innerHTML = `
                <td><input type="checkbox" class="form-check-input" data-field="seleccionado" ${row.seleccionado ? 'checked' : ''}></td>
                <td>${row.docEntry}</td>
                <td>${row.lineId}</td>
                <td>${row.numAtCard}</td>
                <td>${row.concepto}</td>
                <td>${row.tienda}</td>
                <td>${renderSelect('U_MGS_CL_TIPGAE', row.U_MGS_CL_TIPGAE, state.tiposGae)}</td>
                <td><input type="checkbox" class="form-check-input" data-field="U_MGS_CL_AUTORI" ${row.U_MGS_CL_AUTORI === 'SI' ? 'checked' : ''}></td>
                <td>${renderSelect('U_MGS_CL_TIPGAS', row.U_MGS_CL_TIPGAS, state.tiposGasto)}</td>
                <td>${renderSelect('U_MGS_CL_TIPMOP', row.U_MGS_CL_TIPMOP, state.motivos)}</td>
                <td><input type="number" step="0.01" class="form-control form-control-sm input-inline input-importe" data-field="U_MGS_CL_IMPORT" value="${row.U_MGS_CL_IMPORT}"></td>
                <td><input type="date" class="form-control form-control-sm input-inline" data-field="U_MGS_CL_FEPRM" value="${row.U_MGS_CL_FEPRM}"></td>
                <td>${row.U_MGS_CL_SOLICI}</td>
                <td><input type="checkbox" class="form-check-input" data-field="U_MGS_CL_VALIDO" ${row.U_MGS_CL_VALIDO === 'SI' ? 'checked' : ''}></td>
                <td><span class="estado"></span></td>
                <td class="col-error"></td>
            `;

            elements.tablaBody.appendChild(tr);
            updateRowStatus(tr, row);
        });

        const anyError = state.rows.some((row) => row.mensajeError);
        setErrorColumnVisible(anyError);
    };

    const renderSelect = (field, value, items) => {
        const options = items.map((item) => {
            const selected = item.Code === value ? 'selected' : '';
            return `<option value="${item.Code}" ${selected}>${item.Name}</option>`;
        }).join('');

        return `
            <select class="form-select form-select-sm select-inline" data-field="${field}">
                <option value="">Seleccionar</option>
                ${options}
            </select>
        `;
    };

    const mapRow = (item) => {
        return {
            baseDatos: item.BaseDatos || '',
            objectType: item.ObjectType || '',
            docEntry: item.DocEntry || '',
            lineId: item.LineId || '',
            numAtCard: item.NumAtCard || '',
            concepto: item.Concepto || '',
            tienda: item.Tienda || '',
            U_MGS_CL_TIPGAE: item.U_MGS_CL_TIPGAE || '',
            U_MGS_CL_AUTORI: item.U_MGS_CL_AUTORI === 'SI' ? 'SI' : 'NO',
            U_MGS_CL_TIPGAS: item.U_MGS_CL_TIPGAS || '',
            U_MGS_CL_TIPMOP: item.U_MGS_CL_TIPMOP || '',
            U_MGS_CL_IMPORT: item.U_MGS_CL_IMPORT ?? 0,
            U_MGS_CL_FEPRM: item.U_MGS_CL_FEPRM || '',
            U_MGS_CL_SOLICI: item.U_MGS_CL_SOLICI || '',
            U_MGS_CL_VALIDO: item.U_MGS_CL_VALIDO === 'SI' ? 'SI' : 'NO',
            pendiente: item.Pendiente === 'SI',
            mensajeError: item.MensajeError || '',
            estado: item.Pendiente === 'SI' ? 'Pendiente' : (item.MensajeError ? 'Error' : 'OK'),
            seleccionado: false
        };
    };

    const loadCatalogos = async () => {
        try {
            const [tiendas, tiposGae, tiposGasto, motivos] = await Promise.all([
                fetchJson(api.tiendas),
                fetchJson(api.tiposGae),
                fetchJson(api.tiposGasto),
                fetchJson(api.motivos)
            ]);

            state.tiendas = tiendas || [];
            state.tiposGae = tiposGae || [];
            state.tiposGasto = tiposGasto || [];
            state.motivos = motivos || [];

            renderTiendas();
            renderSelectOptions(elements.filtroTipoGae, state.tiposGae, 'Todos');
            renderSelectOptions(elements.filtroTipoGasto, state.tiposGasto, 'Todos');
            renderSelectOptions(elements.filtroMotivo, state.motivos, 'Todos');
            updateTiendasLabel();
            updateLastFilters();
        } catch (error) {
            showAlert(connectionMessage, 'warning');
        }
    };

    const buildFiltroPayload = () => {
        const factura = elements.filtroFactura.value.trim();
        const concepto = elements.filtroConcepto.value.trim();
        const tipoGae = elements.filtroTipoGae.value;
        const tipoGasto = elements.filtroTipoGasto.value;
        const motivo = elements.filtroMotivo.value;
        return `${factura}|${concepto}|${tipoGae}|${tipoGasto}|${motivo}`;
    };

    const buscar = async () => {
        clearAlerts();
        setLoading(true, 'Buscando...');
        try {
            const params = new URLSearchParams({
                fechaDesde: elements.filtroDesde.value,
                fechaHasta: elements.filtroHasta.value,
                tiendas: getSelectedTiendas().join(','),
                filtros: buildFiltroPayload()
            });

            const data = await fetchJson(`${api.buscar}?${params.toString()}`);
            state.rows = (data || []).map(mapRow);
            state.pendientes = state.rows.filter((row) => row.pendiente).length;
            state.originalRows = state.rows.map((row) => ({ ...row }));
            updatePendingBadge();
            renderRows();
            updateLastFilters();
        } catch (error) {
            showAlert(navigator.onLine ? error.message : connectionMessage, 'danger');
        } finally {
            setLoading(false);
        }
    };

    const actualizarTodo = async () => {
        clearAlerts();
        if (!state.rows.length) {
            showAlert('No hay filas para actualizar.', 'warning');
            return;
        }

        const pendientes = state.rows.filter((row) => row.pendiente);
        if (!pendientes.length) {
            showAlert('No hay cambios pendientes.', 'info');
            return;
        }

        setLoading(true, 'Actualizando...');
        try {
            const payload = {
                items: pendientes.map((row) => ({
                    baseDatos: row.baseDatos,
                    objectType: row.objectType,
                    docEntry: row.docEntry,
                    lineId: row.lineId,
                    U_MGS_CL_TIPGAE: row.U_MGS_CL_TIPGAE,
                    U_MGS_CL_AUTORI: row.U_MGS_CL_AUTORI,
                    U_MGS_CL_TIPGAS: row.U_MGS_CL_TIPGAS,
                    U_MGS_CL_TIPMOP: row.U_MGS_CL_TIPMOP,
                    U_MGS_CL_IMPORT: row.U_MGS_CL_IMPORT,
                    U_MGS_CL_FEPRM: row.U_MGS_CL_FEPRM,
                    U_MGS_CL_VALIDO: row.U_MGS_CL_VALIDO
                }))
            };

            const results = await fetchJson(api.actualizar, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            let hasErrors = false;
            (results || []).forEach((result) => {
                const idx = state.rows.findIndex((row) =>
                    row.baseDatos === result.BaseDatos &&
                    row.objectType === result.ObjectType &&
                    row.docEntry === result.DocEntry &&
                    row.lineId === result.LineId
                );
                if (idx === -1) return;
                const row = state.rows[idx];
                if (result.Ok) {
                    clearPendiente(row);
                    row.estado = 'OK';
                    row.mensajeError = '';
                } else {
                    hasErrors = true;
                    row.estado = 'Error';
                    row.mensajeError = result.Message || 'Error al actualizar.';
                    ensurePendiente(row);
                }
                const rowEl = elements.tablaBody.querySelector(`tr[data-index="${idx}"]`);
                updateRowStatus(rowEl, row);
            });

            if (hasErrors) {
                showAlert('Se encontraron errores al actualizar. Revise la columna de mensajes.', 'danger');
                setErrorColumnVisible(true);
            } else {
                setErrorColumnVisible(false);
                showAlert('Actualización realizada correctamente. Use Buscar para refrescar los datos.', 'success');
            }
        } catch (error) {
            showAlert(navigator.onLine ? error.message : connectionMessage, 'danger');
        } finally {
            setLoading(false);
        }
    };

    const exportar = () => {
        if (!state.rows.length) {
            showAlert('No hay datos para exportar.', 'warning');
            return;
        }
        const headers = [
            'Código interno',
            'Línea',
            'N° Factura',
            'Concepto',
            'Tienda',
            'GAE',
            'Autorizado',
            'Tipo de gasto',
            'Motivo de gasto',
            'Importe',
            'Fecha PRM',
            'Solicitado por',
            'No válido',
            'Estado',
            'Mensaje'
        ];
        const rows = state.rows.map((row) => [
            row.docEntry,
            row.lineId,
            row.numAtCard,
            row.concepto,
            row.tienda,
            row.U_MGS_CL_TIPGAE,
            row.U_MGS_CL_AUTORI,
            row.U_MGS_CL_TIPGAS,
            row.U_MGS_CL_TIPMOP,
            row.U_MGS_CL_IMPORT,
            row.U_MGS_CL_FEPRM,
            row.U_MGS_CL_SOLICI,
            row.U_MGS_CL_VALIDO,
            row.estado,
            row.mensajeError
        ]);

        const csvContent = [headers, ...rows]
            .map((row) => row.map((cell) => `"${String(cell ?? '').replace(/"/g, '""')}"`).join(','))
            .join('\n');

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'administracion-gae.csv';
        link.click();
        URL.revokeObjectURL(url);
    };

    elements.btnBuscar.addEventListener('click', () => {
        confirmDiscard(() => buscar());
    });

    elements.btnActualizarTodo.addEventListener('click', () => {
        actualizarTodo();
    });

    elements.btnExportar.addEventListener('click', () => {
        exportar();
    });

    document.getElementById('tiendaSelTodas').addEventListener('click', (event) => {
        event.preventDefault();
        confirmDiscard(() => {
            elements.tiendasListado.querySelectorAll('input[type="checkbox"]').forEach((input) => {
                input.checked = true;
            });
            updateTiendasLabel();
            updateLastFilters();
        }, restoreFilters);
    });

    document.getElementById('tiendaLimpiar').addEventListener('click', (event) => {
        event.preventDefault();
        confirmDiscard(() => {
            elements.tiendasListado.querySelectorAll('input[type="checkbox"]').forEach((input) => {
                input.checked = false;
            });
            updateTiendasLabel();
            updateLastFilters();
        }, restoreFilters);
    });

    elements.tiendasListado.addEventListener('change', (event) => {
        if (event.target.type !== 'checkbox') return;
        confirmDiscard(() => {
            updateTiendasLabel();
            updateLastFilters();
        }, () => {
            event.target.checked = !event.target.checked;
        });
    });

    const filterInputs = [
        elements.filtroDesde,
        elements.filtroHasta,
        elements.filtroFactura,
        elements.filtroConcepto,
        elements.filtroTipoGae,
        elements.filtroTipoGasto,
        elements.filtroMotivo
    ];

    filterInputs.forEach((input) => {
        input.addEventListener('change', () => {
            confirmDiscard(() => updateLastFilters(), restoreFilters);
        });
    });

    elements.tablaBody.addEventListener('change', (event) => {
        const target = event.target;
        const rowEl = target.closest('tr');
        if (!rowEl) return;
        const index = Number(rowEl.dataset.index);
        const row = state.rows[index];
        if (!row) return;

        const field = target.dataset.field;
        if (!field) return;

        if (field === 'seleccionado') {
            row.seleccionado = target.checked;
            return;
        }

        let value;
        if (target.type === 'checkbox') {
            value = target.checked ? 'SI' : 'NO';
        } else {
            value = target.value;
        }

        row[field] = value;
        setPendiente(row);

        const bulkFields = ['U_MGS_CL_AUTORI', 'U_MGS_CL_TIPGAS', 'U_MGS_CL_TIPMOP', 'U_MGS_CL_TIPGAE'];
        if (row.seleccionado && bulkFields.includes(field)) {
            applyBulkChange(field, value);
        }

        updateRowStatus(rowEl, row);
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

    loadCatalogos();
});
