-- Script consolidado para RusticaPortal: incluye ramas de menú, cuentas,
-- matriz de factores, tiendas activas y otros auxiliares.
alter PROCEDURE MGS_HDB_PE_SP_PORTALWEB (
    IN vTipo NVARCHAR(20),
    IN vParam1 NVARCHAR(50),
    IN vParam2 NVARCHAR(50),
    IN vParam3 NVARCHAR(50),
    IN vParam4 NVARCHAR(50))
AS
BEGIN

    IF vTipo = 'Get_account' THEN 

        SELECT  
            top 1
            T0."Code" AS CardCode, 
            T0."Name" AS NombreCompleto,
            T0."Code"  AS UsuarioID,
            T1."U_MGS_CL_PRFID" AS PerfilId,
            T2."Name" as NombrePerfil,
            IFNULL(T2."U_MGS_CL_IMAGEN", '') AS Popup            
        FROM "@MGS_CL_USPECAB" T0
        inner join "@MGS_CL_USPEDET" T1 ON T0."Code" = T1."Code"
        INNER JOIN "@MGS_CL_PERFIL" T2 ON T1."U_MGS_CL_PRFID" = T2."Code"                        
        WHERE T0."Code"= :vParam1 AND T0."U_MGS_CL_USPASS" = :vParam2;
        
    ELSEIF vTipo = 'Get_menu' THEN
    
        SELECT DISTINCT 
            Id,
            NombreMenu,
            CodigoMenu,
            EsPadre,
            PadreID,
            Ruta,
            Activo,
            OrdenMenu,
            nombre,
            usuario
        FROM (
                    /*SELECT 
                        M."DocEntry" AS Id,  
                        M."Name" AS NombreMenu,
                        M."Code" AS CodigoMenu,
                        M."U_MGS_CL_MNUESP" AS EsPadre,
                        M."U_MGS_CL_MNUPAD" AS PadreID,
                        M."U_MGS_CL_MNURUT" AS Ruta,
                        M."U_MGS_CL_MNUACT" AS Activo,
                        M."U_MGS_CL_MNUOD" AS OrdenMenu,
                        T0."CardName" AS nombre,
                        'RUC - ' ||  T0."LicTradNum"  AS usuario
                    FROM "OCRD" T0
                    JOIN "@MGS_CL_PERFIL" P ON T0."U_MGS_CL_PERFIL" = P."Code"
                    JOIN "@MGS_CL_PFMNDET" PMD ON P."Code" = PMD."Code"
                    JOIN "@MGS_CL_MENU" M ON PMD."U_MGS_CL_PMMNID" = M."Code"
                    WHERE M."U_MGS_CL_MNUACT" = 'SI'
                      AND P."U_MGS_CL_PACTIV" = 'SI'
                      -- Reemplaza este valor por el código real del usuario logueado
                      AND T0."LicTradNum" = :vParam1        
                      
                    UNION ALL*/
                    SELECT 
                        M."DocEntry" AS Id,  
                        M."Name" AS NombreMenu,
                        M."Code" AS CodigoMenu,
                        M."U_MGS_CL_MNUESP" AS EsPadre,
                        M."U_MGS_CL_MNUPAD" AS PadreID,
                        M."U_MGS_CL_MNURUT" AS Ruta,
                        M."U_MGS_CL_MNUACT" AS Activo,
                        M."U_MGS_CL_MNUOD" AS OrdenMenu,
                        U."Code" AS nombre,
                        U."Name"  AS usuario
                    FROM "@MGS_CL_USPECAB" U
                    JOIN "@MGS_CL_USPEDET" UD ON U."Code" = UD."Code"
                    JOIN "@MGS_CL_PFMNDET" PMD ON UD."U_MGS_CL_PRFID" = PMD."Code"
                    JOIN "@MGS_CL_PERFIL" P ON PMD."Code" = P."Code"
                    JOIN "@MGS_CL_MENU" M ON PMD."U_MGS_CL_PMMNID" = M."Code"
                    WHERE U."U_MGS_CL_USPEACT" = 'SI'
                      AND M."U_MGS_CL_MNUACT" = 'SI'
                      AND P."U_MGS_CL_PACTIV" = 'SI'
                      -- Reemplaza este valor por el código real del usuario logueado
                      AND U."Code" = :vParam1             
                    ORDER BY OrdenMenu asc) AS MenusFinales
            ORDER BY OrdenMenu asc;



    ELSEIF vTipo = 'Get_TiendasActivas' THEN

        SELECT
            "PrjCode" AS "Codigo",
            "PrjName" AS "Nombre"
        FROM "OPRJ"
        WHERE "Active" = 'Y'
        ORDER BY "PrjCode";


  ELSEIF vTipo = 'Get_MatrizFactores' THEN

    DECLARE lvTiendas NVARCHAR(5000);
    DECLARE lvSql     NVARCHAR(5000);

-- vParam2 = 'P0045,P0031'
    lvTiendas := '''' || REPLACE(:vParam2, ',', ''',''') || '''';

-- Resultado:  'P0045','P0031'

 IF LENGTH(:vParam2) = 0 THEN
    lvSql := '
        SELECT
            TO_VARCHAR(C."U_MGS_CL_PERIODO", ''YYYY-MM'')  AS "U_MGS_CL_PERIODO",
            D."U_MGS_CL_TIENDA"   AS "U_MGS_CL_TIENDA",
            D."U_MGS_CL_NOMTIE"   AS "U_MGS_CL_NOMTIE",
            C."DocEntry"          AS "DocEntry",
            D."LineId"          AS "LineId",
            D."U_MGS_CL_META"     AS "U_MGS_CL_META",
            D."U_MGS_CL_RENTA"    AS "U_MGS_CL_RENTA",
            D."U_MGS_CL_VAN"      AS "U_MGS_CL_VAN",
            D."U_MGS_CL_ESTPER"   AS "U_MGS_CL_ESTPER",
            D."U_MGS_CL_GASADM"   AS "U_MGS_CL_GASADM",
            D."U_MGS_CL_COMTAR"   AS "U_MGS_CL_COMTAR",
            D."U_MGS_CL_IMPUES"   AS "U_MGS_CL_IMPUES",
            D."U_MGS_CL_REGALI"   AS "U_MGS_CL_REGALI",
            D."U_MGS_CL_AUSER"    AS "U_MGS_CL_AUSER",
            D."U_MGS_CL_AUOPE"    AS "U_MGS_CL_AUOPE",
            D."U_MGS_CL_AUCCC"    AS "U_MGS_CL_AUCCC",
            D."U_MGS_CL_AUADH"    AS "U_MGS_CL_AUADH",
            D."U_MGS_CL_CLIMA"    AS "U_MGS_CL_CLIMA",
            D."U_MGS_CL_RUSTI"    AS "U_MGS_CL_RUSTI",
            D."U_MGS_CL_MELID"    AS "U_MGS_CL_MELID",
            D."U_MGS_CL_ADMGR"    AS "U_MGS_CL_ADMGR",
            D."U_MGS_CL_EXGES"    AS "U_MGS_CL_EXGES",
            D."U_MGS_CL_EXSER"    AS "U_MGS_CL_EXSER",
            D."U_MGS_CL_EXMAR"    AS "U_MGS_CL_EXMAR",
            D."U_MGS_CL_PRIMARY"  AS "U_MGS_CL_PRIMARY"
        FROM "@MGS_CL_FACCAB" C
        JOIN "@MGS_CL_FACDET" D
          ON D."DocEntry" = C."DocEntry"
        WHERE TO_VARCHAR(C."U_MGS_CL_PERIODO", ''YYYY-MM'') = ''' || :vParam1 || '''
        ORDER BY
            D."LineId", D."U_MGS_CL_TIENDA", D."U_MGS_CL_NOMTIE"
    ';
 ELSE
    lvSql := '
        SELECT
            TO_VARCHAR(C."U_MGS_CL_PERIODO", ''YYYY-MM'')  AS "U_MGS_CL_PERIODO",
            D."U_MGS_CL_TIENDA"   AS "U_MGS_CL_TIENDA",
            D."U_MGS_CL_NOMTIE"   AS "U_MGS_CL_NOMTIE",
            C."DocEntry"          AS "DocEntry",
            D."LineId"          AS "LineId",
            D."U_MGS_CL_META"     AS "U_MGS_CL_META",
            D."U_MGS_CL_RENTA"    AS "U_MGS_CL_RENTA",
            D."U_MGS_CL_VAN"      AS "U_MGS_CL_VAN",
            D."U_MGS_CL_ESTPER"   AS "U_MGS_CL_ESTPER",
            D."U_MGS_CL_GASADM"   AS "U_MGS_CL_GASADM",
            D."U_MGS_CL_COMTAR"   AS "U_MGS_CL_COMTAR",
            D."U_MGS_CL_IMPUES"   AS "U_MGS_CL_IMPUES",
            D."U_MGS_CL_REGALI"   AS "U_MGS_CL_REGALI",
            D."U_MGS_CL_AUSER"    AS "U_MGS_CL_AUSER",
            D."U_MGS_CL_AUOPE"    AS "U_MGS_CL_AUOPE",
            D."U_MGS_CL_AUCCC"    AS "U_MGS_CL_AUCCC",
            D."U_MGS_CL_AUADH"    AS "U_MGS_CL_AUADH",
            D."U_MGS_CL_CLIMA"    AS "U_MGS_CL_CLIMA",
            D."U_MGS_CL_RUSTI"    AS "U_MGS_CL_RUSTI",
            D."U_MGS_CL_MELID"    AS "U_MGS_CL_MELID",
            D."U_MGS_CL_ADMGR"    AS "U_MGS_CL_ADMGR",
            D."U_MGS_CL_EXGES"    AS "U_MGS_CL_EXGES",
            D."U_MGS_CL_EXSER"    AS "U_MGS_CL_EXSER",
            D."U_MGS_CL_EXMAR"    AS "U_MGS_CL_EXMAR",
            D."U_MGS_CL_PRIMARY"  AS "U_MGS_CL_PRIMARY"
        FROM "@MGS_CL_FACCAB" C
        JOIN "@MGS_CL_FACDET" D
          ON D."DocEntry" = C."DocEntry"
        WHERE TO_VARCHAR(C."U_MGS_CL_PERIODO", ''YYYY-MM'') = ''' || :vParam1 || '''
          AND D."U_MGS_CL_TIENDA" IN (' || lvTiendas || ')
        ORDER BY
            D."LineId", D."U_MGS_CL_TIENDA", D."U_MGS_CL_NOMTIE"
    ';
 END IF;

    EXECUTE IMMEDIATE :lvSql;

  ELSEIF vTipo = 'Get_SiguientePeriodoFactores' THEN

    DECLARE lvPeriodoActual NVARCHAR(7);
    DECLARE lvPeriodoSiguiente NVARCHAR(7);

    SELECT IFNULL(MAX(TO_VARCHAR("U_MGS_CL_PERIODO", 'YYYY-MM')), '') INTO lvPeriodoActual FROM "@MGS_CL_FACCAB";

    IF :lvPeriodoActual = '' THEN
        lvPeriodoActual := TO_VARCHAR(CURRENT_DATE, 'YYYY-MM');
        lvPeriodoSiguiente := TO_VARCHAR(ADD_MONTHS(CURRENT_DATE, 1), 'YYYY-MM');
    ELSE
        SELECT TO_VARCHAR(ADD_MONTHS(TO_DATE(:lvPeriodoActual || '-01'), 1), 'YYYY-MM') INTO lvPeriodoSiguiente FROM DUMMY;
    END IF;

    SELECT :lvPeriodoActual AS "PeriodoBase",
           :lvPeriodoSiguiente AS "PeriodoSiguiente"
    FROM DUMMY;



    
    ELSEIF vTipo = 'Get_fechaRecepcion' THEN
    
        /*SELECT 
        T1."U_MGS_CL_FERINI",        
        T1."U_MGS_CL_FERFIN",        
        CASE TO_VARCHAR(T1."U_MGS_CL_FERFIN", 'MM')        
        WHEN '01' THEN 'Enero'        
        WHEN '02' THEN 'Febrero'        
        WHEN '03' THEN 'Marzo'        
        WHEN '04' THEN 'Abril'        
        WHEN '05' THEN 'Mayo'        
        WHEN '06' THEN 'Junio'        
        WHEN '07' THEN 'Julio'        
        WHEN '08' THEN 'Agosto'        
        WHEN '09' THEN 'Septiembre'        
        WHEN '10' THEN 'Octubre'        
        WHEN '11' THEN 'Noviembre'        
        WHEN '12' THEN 'Diciembre'        
        END AS Mes,
        -- Concatenar día de semana + día numérico        
        CASE DAYOFWEEK(T1."U_MGS_CL_FERFIN")        
        WHEN 1 THEN 'Domingo'        
        WHEN 2 THEN 'Lunes'        
        WHEN 3 THEN 'Martes'        
        WHEN 4 THEN 'Miércoles'
        WHEN 5 THEN 'Jueves'        
        WHEN 6 THEN 'Viernes'        
        WHEN 7 THEN 'Sábado'        
        END || ' ' || TO_VARCHAR(EXTRACT(DAY FROM T1."U_MGS_CL_FERFIN")) AS DiaSemanaNumero

        FROM "@MGS_CL_FERECAB" T0    
        INNER JOIN "@MGS_CL_FEREDET" T1 ON T1."Code" = T0."Code"
        WHERE "U_MGS_CL_REGANO" = EXTRACT(YEAR FROM CURRENT_DATE)        
        ORDER BY EXTRACT(MONTH FROM T1."U_MGS_CL_FERFIN");
        */
    ELSEIF vTipo = 'Get_contactoEmpresa' THEN
    
        SELECT TOP 1
          (SELECT T1."U_MGS_CL_VALOR" FROM "@MGS_CL_CONDET" T1 WHERE  T1."Code" = T0."Code" and "U_MGS_CL_PARAM"  = 'telefono') AS Phone ,
          (SELECT T1."U_MGS_CL_VALOR" FROM "@MGS_CL_CONDET" T1  WHERE T1."Code" = T0."Code" and "U_MGS_CL_PARAM"  = 'correo')  AS Email,
          IFNULL(T0."U_MGS_CL_LOGO", '') AS LogoUrl
        FROM "@MGS_CL_CONFIG" T0;
                
    END IF;
    
    
END;
