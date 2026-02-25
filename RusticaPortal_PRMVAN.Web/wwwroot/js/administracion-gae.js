document.addEventListener('DOMContentLoaded', () => {
    const api = { tiendas: '/AdministracionGAE/Tiendas', tiposGae: '/AdministracionGAE/TiposGae', tiposGasto: '/AdministracionGAE/TiposGasto', motivos: '/AdministracionGAE/MotivosGasto', buscar: '/AdministracionGAE/Buscar', actualizar: '/AdministracionGAE/ActualizarTodo' };
    const state = { tiendas: [], tiposGae: [], tiposGasto: [], motivos: [], rows: [], originalRows: [], pendientes: 0, pendingAction: null, page: 1, pageSize: 50 };
    const el = {
        periodo: document.getElementById('filtroPeriodo'), desde: document.getElementById('filtroDesde'), hasta: document.getElementById('filtroHasta'),
        factura: document.getElementById('filtroFactura'), concepto: document.getElementById('filtroConcepto'), tipoGasto: document.getElementById('filtroTipoGasto'), motivo: document.getElementById('filtroMotivo'),
        tiendasBtn: document.getElementById('tiendasDropdownBtn'), tiendasEstado: document.getElementById('tiendasEstado'), tiendasListado: document.getElementById('tiendasListado'),
        tipoGaeBtn: document.getElementById('tipoGaeDropdownBtn'), tipoGaeEstado: document.getElementById('tipoGaeEstado'), tipoGaeListado: document.getElementById('tipoGaeListado'),
        btnBuscar: document.getElementById('btnBuscar'), btnActualizarTodo: document.getElementById('btnActualizarTodo'), pendingBadge: document.getElementById('pendingBadge'),
        tablaBody: document.querySelector('#tablaGae tbody'), colErrorHeader: document.querySelector('#tablaGae thead .col-error'), estadoHeader: document.querySelector('#tablaGae thead .col-estado'), alertContainer: document.getElementById('alertContainer'),
        loadingOverlay: document.getElementById('loadingOverlay'), loadingMessage: document.getElementById('loadingMessage'), modalCambios: document.getElementById('modalCambios'), btnDescartar: document.getElementById('btnDescartar'),
        pageSize: document.getElementById('pageSize'), pageInfo: document.getElementById('pageInfo'), btnPrevPage: document.getElementById('btnPrevPage'), btnNextPage: document.getElementById('btnNextPage')
    };
    const offline = 'No tiene conexión. Intente nuevamente.';
    const get = (obj, keys) => { for (const k of keys) if (obj && obj[k] !== undefined && obj[k] !== null) return obj[k]; return ''; };
    const selected = (list) => [...list.querySelectorAll('input[type="checkbox"]:checked')].map(i => i.value);
    const setLoading = (s, m = 'Procesando...') => { el.loadingMessage.textContent = m; el.loadingOverlay.classList.toggle('d-none', !s); };
    const alert = (m, t = 'info') => el.alertContainer.insertAdjacentHTML('beforeend', `<div class="alert alert-${t} alert-dismissible fade show" role="alert">${m}<button type="button" class="btn-close" data-bs-dismiss="alert"></button></div>`);
    const clearAlerts = () => el.alertContainer.innerHTML = '';
    const fetchJson = async (url, options) => { if (!navigator.onLine) throw new Error(offline); const r = await fetch(url, options); const j = await r.json().catch(() => ({})); if (!r.ok) throw new Error(j.message || j.Message || JSON.stringify(j)); return j; };
    const updBadge = () => el.pendingBadge.textContent = String(state.pendientes);
    const setErrVisible = (show) => { el.colErrorHeader.classList.toggle('d-none', !show); el.estadoHeader.classList.toggle('d-none', !show); document.querySelectorAll('#tablaGae tbody .col-error').forEach(c => c.classList.toggle('d-none', !show)); document.querySelectorAll('#tablaGae tbody .col-estado').forEach(c => c.classList.toggle('d-none', !show)); };
    const isPeriodValid = () => { if (!el.periodo.value || !el.desde.value || !el.hasta.value) return true; const [y,m] = el.periodo.value.split('-').map(Number); const min = new Date(y, m-1, 1), max = new Date(y, m, 0); const d = new Date(el.desde.value), h = new Date(el.hasta.value); return d >= min && d <= max && h >= min && h <= max; };
    const validatePeriodOrAlert = () => { if (isPeriodValid()) return true; alert('Las fechas Desde/Hasta deben estar dentro del Periodo seleccionado.', 'warning'); return false; };
    const renderChecks = (items, listEl, stateEl, keys, emptyText) => {
        listEl.innerHTML = '';
        if (!items.length) { stateEl.textContent = emptyText; return; }
        stateEl.textContent = `Registros: ${items.length}`;
        items.forEach((it, i) => { const c = get(it, keys.code), n = get(it, keys.name); listEl.insertAdjacentHTML('beforeend', `<div class="form-check"><input class="form-check-input" type="checkbox" value="${c}" id="${listEl.id}-${i}"><label class="form-check-label" for="${listEl.id}-${i}">${c} - ${n}</label></div>`); });
    };
    const updateMultiLabel = (btn, list) => { const n = selected(list).length; const t = list.querySelectorAll('input[type="checkbox"]').length; btn.textContent = n === 0 ? 'Sin selección' : (n === t ? 'Todas' : `${n} seleccionadas`); };
    const renderCombo = (sel, items) => { sel.innerHTML = '<option value="">TODOS</option>'; items.forEach(i => sel.insertAdjacentHTML('beforeend', `<option value="${get(i,['Code','code','Codigo','codigo'])}">${get(i,['Name','name','Nombre','nombre'])}</option>`)); };
    const renderSelect = (field, value, items) => `<select class="form-select form-select-sm select-inline" data-field="${field}"><option value="">Seleccionar</option>${items.map(i => { const c = get(i,['Code','code','Codigo','codigo']); const n = get(i,['Name','name','Nombre','nombre']); return `<option value="${c}" ${c === value ? 'selected':''}>${n}</option>`; }).join('')}</select>`;

    const mapRow = (item) => ({ idEmpresa: get(item,['IdEmpresa','idEmpresa']), nombreEmpresa: get(item,['NombreEmpresa','nombreEmpresa']), baseDatos: get(item,['BaseDatos','baseDatos','NombreEmpresa']), objectType: get(item,['ObjectType','objectType']), docEntry: get(item,['DocEntry','docEntry']), lineId: get(item,['LineId','lineId']), numAtCard: get(item,['NumAtCard','numAtCard']), concepto: get(item,['Concepto','concepto']), tienda: get(item,['Tienda','tienda']), U_MGS_CL_TIPGAE: get(item,['U_MGS_CL_TIPGAE']), U_MGS_CL_AUTORI: get(item,['U_MGS_CL_AUTORI']) === 'SI' ? 'SI':'NO', U_MGS_CL_TIPGAS: get(item,['U_MGS_CL_TIPGAS']), U_MGS_CL_TIPMOP: get(item,['U_MGS_CL_TIPMOP']), U_MGS_CL_IMPORT: Number(get(item,['U_MGS_CL_IMPORT']) || 0), U_MGS_CL_FEPRM: get(item,['U_MGS_CL_FEPRM']), fecFiltro: get(item,['FecFiltro','FECFILTRO']), U_MGS_CL_SOLICI: get(item,['U_MGS_CL_SOLICI']), U_MGS_CL_VALIDO: get(item,['U_MGS_CL_VALIDO']) === 'SI' ? 'SI':'NO', pendiente: get(item,['Pendiente']) === 'SI', mensajeError: get(item,['MensajeError','mensajeError']), estado: get(item,['Pendiente']) === 'SI' ? 'Pendiente' : (get(item,['MensajeError']) ? 'Error' : 'OK'), seleccionado: false });

    const updateRowStatus = (tr, row) => { tr.classList.toggle('fila-pendiente', row.pendiente); tr.querySelector('.estado').textContent = row.estado; tr.querySelector('.col-error').textContent = row.mensajeError || ''; };
    const renderRows = () => {
        el.tablaBody.innerHTML = '';
        state.rows.forEach((row, i) => {
            const tr = document.createElement('tr'); tr.dataset.index = String(i);
            tr.innerHTML = `<td><input type="checkbox" class="form-check-input" data-field="seleccionado" ${row.seleccionado ? 'checked':''}></td><td>${row.docEntry}</td><td>${row.lineId}</td><td>${row.numAtCard}</td><td>${row.concepto}</td><td>${row.tienda}</td><td>${renderSelect('U_MGS_CL_TIPGAE', row.U_MGS_CL_TIPGAE, state.tiposGae)}</td><td><input type="checkbox" class="form-check-input" data-field="U_MGS_CL_AUTORI" ${row.U_MGS_CL_AUTORI === 'SI' ? 'checked':''}></td><td>${renderSelect('U_MGS_CL_TIPGAS', row.U_MGS_CL_TIPGAS, state.tiposGasto)}</td><td>${renderSelect('U_MGS_CL_TIPMOP', row.U_MGS_CL_TIPMOP, state.motivos)}</td><td><input type="number" step="0.01" class="form-control form-control-sm input-inline input-importe" data-field="U_MGS_CL_IMPORT" value="${row.U_MGS_CL_IMPORT}"></td><td><input type="date" class="form-control form-control-sm input-inline" data-field="U_MGS_CL_FEPRM" value="${row.fecFiltro || row.U_MGS_CL_FEPRM}"></td><td>${row.U_MGS_CL_SOLICI}</td><td><input type="checkbox" class="form-check-input" data-field="U_MGS_CL_VALIDO" ${row.U_MGS_CL_VALIDO === 'SI' ? 'checked':''}></td><td class="col-estado"><span class="estado"></span></td><td class="col-error"></td>`;
            el.tablaBody.appendChild(tr); updateRowStatus(tr, row);
        });
        setErrVisible(state.rows.some(r => r.mensajeError));
        el.pageInfo.textContent = `Página ${state.page}`;
    };

    const discardChanges = () => { state.rows = state.originalRows.map(r => ({ ...r, seleccionado: false })); state.pendientes = 0; updBadge(); renderRows(); };
    const confirmDiscard = (ok, cancel) => { if (!state.pendientes) return ok(); state.pendingAction = { ok, cancel }; bootstrap.Modal.getOrCreateInstance(el.modalCambios).show(); };
    el.btnDescartar.addEventListener('click', () => { const a = state.pendingAction; state.pendingAction = null; discardChanges(); bootstrap.Modal.getOrCreateInstance(el.modalCambios).hide(); a?.ok?.(); });
    el.modalCambios.addEventListener('hidden.bs.modal', () => { const a = state.pendingAction; state.pendingAction = null; a?.cancel?.(); });

    const filtroPayload = () => `${el.factura.value.trim()}|${el.concepto.value.trim()}|${selected(el.tipoGaeListado).join(',')}|${el.tipoGasto.value}|${el.motivo.value}`;

    const buscar = async () => {
        clearAlerts(); if (!validatePeriodOrAlert()) return;
        setLoading(true, 'Buscando...');
        try {
            const params = new URLSearchParams({ fechaDesde: el.desde.value, fechaHasta: el.hasta.value, tiendas: selected(el.tiendasListado).join(','), filtros: filtroPayload(), page: String(state.page), pageSize: String(state.pageSize) });
            const data = await fetchJson(`${api.buscar}?${params.toString()}`);
            state.rows = (data || []).map(mapRow); state.originalRows = state.rows.map(r => ({ ...r })); state.pendientes = state.rows.filter(r => r.pendiente).length; updBadge(); renderRows();
        } catch (e) { alert(e.message || offline, 'danger'); }
        finally { setLoading(false); }
    };

    const actualizarTodo = async () => {
        clearAlerts();
        if (!navigator.onLine) return alert(offline, 'warning');
        const pendientes = state.rows.filter(r => r.pendiente);
        if (!pendientes.length) return alert('No hay cambios pendientes.', 'info');
        setLoading(true, 'Actualizando...');
        try {
            const results = await fetchJson(api.actualizar, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ items: pendientes.map(r => ({ idEmpresa: r.idEmpresa, nombreEmpresa: r.nombreEmpresa, baseDatos: r.baseDatos, objectType: r.objectType, docEntry: r.docEntry, lineId: r.lineId, U_MGS_CL_TIPGAE: r.U_MGS_CL_TIPGAE, U_MGS_CL_AUTORI: r.U_MGS_CL_AUTORI, U_MGS_CL_TIPGAS: r.U_MGS_CL_TIPGAS, U_MGS_CL_TIPMOP: r.U_MGS_CL_TIPMOP, U_MGS_CL_IMPORT: r.U_MGS_CL_IMPORT, U_MGS_CL_FEPRM: r.U_MGS_CL_FEPRM, U_MGS_CL_VALIDO: r.U_MGS_CL_VALIDO })) }) });
            let hasErr = false;
            (results || []).forEach(res => {
                const row = state.rows.find(r => r.baseDatos === res.BaseDatos && r.objectType === res.ObjectType && r.docEntry === res.DocEntry && r.lineId === res.LineId);
                if (!row) return;
                if (res.Ok) { row.pendiente = false; row.estado = 'OK'; row.mensajeError = ''; }
                else { hasErr = true; row.pendiente = true; row.estado = 'Error'; row.mensajeError = res.Message || 'Error'; }
            });
            state.pendientes = state.rows.filter(r => r.pendiente).length; updBadge(); renderRows();
            if (hasErr) { setErrVisible(true); alert('Se encontraron errores al actualizar. Revise Estado/Mensaje.', 'danger'); }
            else { setErrVisible(false); alert('Actualización realizada correctamente. Se recargará la búsqueda.', 'success'); await buscar(); }
        } catch (e) { alert(e.message || offline, 'danger'); }
        finally { setLoading(false); }
    };

    const changePeriodo = () => {
        if (!el.periodo.value) return;
        const [y,m] = el.periodo.value.split('-').map(Number); el.desde.value = `${y}-${String(m).padStart(2,'0')}-01`; el.hasta.value = new Date(y, m, 0).toISOString().slice(0,10);
    };

    const loadCatalogos = async () => {
        try {
            const [tiendas, tiposGae, tiposGasto, motivos] = await Promise.all([fetchJson(api.tiendas), fetchJson(api.tiposGae), fetchJson(api.tiposGasto), fetchJson(api.motivos)]);
            state.tiendas = tiendas || []; state.tiposGae = tiposGae || []; state.tiposGasto = tiposGasto || []; state.motivos = motivos || [];
            renderChecks(state.tiendas, el.tiendasListado, el.tiendasEstado, { code:['Codigo','codigo','Code','code'], name:['Nombre','nombre','Name','name'] }, 'No hay tiendas disponibles');
            renderChecks(state.tiposGae, el.tipoGaeListado, el.tipoGaeEstado, { code:['Code','code','Codigo','codigo'], name:['Name','name','Nombre','nombre'] }, 'No hay tipos GAE disponibles');
            renderCombo(el.tipoGasto, state.tiposGasto); renderCombo(el.motivo, state.motivos);
            updateMultiLabel(el.tiendasBtn, el.tiendasListado); updateMultiLabel(el.tipoGaeBtn, el.tipoGaeListado);
        } catch (e) { alert(e.message || offline, 'warning'); }
    };

    document.getElementById('tiendaSelTodas').addEventListener('click', e => { e.preventDefault(); confirmDiscard(() => { el.tiendasListado.querySelectorAll('input').forEach(i => i.checked = true); updateMultiLabel(el.tiendasBtn, el.tiendasListado); }); });
    document.getElementById('tiendaLimpiar').addEventListener('click', e => { e.preventDefault(); confirmDiscard(() => { el.tiendasListado.querySelectorAll('input').forEach(i => i.checked = false); updateMultiLabel(el.tiendasBtn, el.tiendasListado); }); });
    document.getElementById('tipoGaeSelTodos').addEventListener('click', e => { e.preventDefault(); confirmDiscard(() => { el.tipoGaeListado.querySelectorAll('input').forEach(i => i.checked = true); updateMultiLabel(el.tipoGaeBtn, el.tipoGaeListado); }); });
    document.getElementById('tipoGaeLimpiar').addEventListener('click', e => { e.preventDefault(); confirmDiscard(() => { el.tipoGaeListado.querySelectorAll('input').forEach(i => i.checked = false); updateMultiLabel(el.tipoGaeBtn, el.tipoGaeListado); }); });
    [el.tiendasListado, el.tipoGaeListado].forEach(list => list.addEventListener('change', () => { updateMultiLabel(list === el.tiendasListado ? el.tiendasBtn : el.tipoGaeBtn, list); }));

    [el.periodo, el.desde, el.hasta, el.factura, el.concepto, el.tipoGasto, el.motivo].forEach(input => input.addEventListener('change', () => confirmDiscard(() => { if (input === el.periodo) changePeriodo(); })));
    el.btnBuscar.addEventListener('click', () => confirmDiscard(() => { state.page = 1; buscar(); }));
    el.btnActualizarTodo.addEventListener('click', actualizarTodo);

    el.pageSize.addEventListener('change', () => confirmDiscard(() => { state.pageSize = Number(el.pageSize.value); state.page = 1; state.rows = []; renderRows(); buscar(); }));
    el.btnPrevPage.addEventListener('click', () => { if (state.page <= 1) return; confirmDiscard(() => { state.page--; state.rows = []; renderRows(); buscar(); }); });
    el.btnNextPage.addEventListener('click', () => confirmDiscard(() => { state.page++; state.rows = []; renderRows(); buscar(); }));

    el.tablaBody.addEventListener('change', (event) => {
        const tr = event.target.closest('tr'); if (!tr) return;
        const row = state.rows[Number(tr.dataset.index)]; const f = event.target.dataset.field; if (!row || !f) return;
        if (f === 'seleccionado') { row.seleccionado = event.target.checked; return; }
        row[f] = event.target.type === 'checkbox' ? (event.target.checked ? 'SI':'NO') : event.target.value; row.pendiente = true; row.estado = 'Pendiente'; row.mensajeError = ''; state.pendientes = state.rows.filter(r => r.pendiente).length; updBadge(); updateRowStatus(tr, row);
    });

    document.addEventListener('click', (event) => { const link = event.target.closest('a'); if (!link || link.getAttribute('href')?.startsWith('#') || !state.pendientes) return; event.preventDefault(); confirmDiscard(() => window.location.href = link.href); });
    window.addEventListener('beforeunload', (event) => { if (!state.pendientes) return; event.preventDefault(); event.returnValue = 'Hay cambios pendientes. ¿Desea descartar los cambios?'; });

    setErrVisible(false);
    loadCatalogos();
});
