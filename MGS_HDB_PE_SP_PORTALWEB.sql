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
    --select :vParam1 from DUMMY;
    --select :lvTiendas from DUMMY;
    
-- Resultado:  'P0045','P0031'

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

    EXECUTE IMMEDIATE :lvSql;


    ELSEIF vTipo = 'Get_FactoresNuevo' THEN

        DECLARE lvPeriodoBaseDate DATE;
        DECLARE lvPeriodoBase NVARCHAR(10);
        DECLARE lvPeriodoDestino NVARCHAR(10);

        -- Determinar el último periodo registrado y el siguiente (periodo destino)
        SELECT MAX("U_MGS_CL_PERIODO")
          INTO lvPeriodoBaseDate
          FROM "@MGS_CL_FACCAB";

        IF lvPeriodoBaseDate IS NULL THEN
            lvPeriodoBase := TO_VARCHAR(CURRENT_DATE, 'YYYY-MM');
            lvPeriodoDestino := TO_VARCHAR(ADD_MONTHS(CURRENT_DATE, 1), 'YYYY-MM');
        ELSE
            lvPeriodoBase := TO_VARCHAR(lvPeriodoBaseDate, 'YYYY-MM');
            lvPeriodoDestino := TO_VARCHAR(ADD_MONTHS(lvPeriodoBaseDate, 1), 'YYYY-MM');
        END IF;


        SELECT
            :lvPeriodoBase      AS "U_MGS_CL_PERIODO",
            :lvPeriodoDestino   AS "U_MGS_CL_PERIODO_DEST",
            P."PrjCode"        AS "U_MGS_CL_TIENDA",
            P."PrjName"        AS "U_MGS_CL_NOMTIE",
            IFNULL(F."DocEntry", 0)           AS "DocEntry",
            IFNULL(F."LineId", 0)             AS "LineId",
            IFNULL(F."U_MGS_CL_META", 0)      AS "U_MGS_CL_META",
            IFNULL(F."U_MGS_CL_RENTA", 0)     AS "U_MGS_CL_RENTA",
            IFNULL(F."U_MGS_CL_VAN", 0)       AS "U_MGS_CL_VAN",
            IFNULL(F."U_MGS_CL_ESTPER", 0)    AS "U_MGS_CL_ESTPER",
            IFNULL(F."U_MGS_CL_GASADM", 0)    AS "U_MGS_CL_GASADM",
            IFNULL(F."U_MGS_CL_COMTAR", 0)    AS "U_MGS_CL_COMTAR",
            IFNULL(F."U_MGS_CL_IMPUES", 0)    AS "U_MGS_CL_IMPUES",
            IFNULL(F."U_MGS_CL_REGALI", 0)    AS "U_MGS_CL_REGALI",
            IFNULL(F."U_MGS_CL_AUSER", 0)     AS "U_MGS_CL_AUSER",
            IFNULL(F."U_MGS_CL_AUOPE", 0)     AS "U_MGS_CL_AUOPE",
            IFNULL(F."U_MGS_CL_AUCCC", 0)     AS "U_MGS_CL_AUCCC",
            IFNULL(F."U_MGS_CL_AUADH", 0)     AS "U_MGS_CL_AUADH",
            IFNULL(F."U_MGS_CL_CLIMA", 0)     AS "U_MGS_CL_CLIMA",
            IFNULL(F."U_MGS_CL_RUSTI", 0)     AS "U_MGS_CL_RUSTI",
            IFNULL(F."U_MGS_CL_MELID", 0)     AS "U_MGS_CL_MELID",
            IFNULL(F."U_MGS_CL_ADMGR", 0)     AS "U_MGS_CL_ADMGR",
            IFNULL(F."U_MGS_CL_EXGES", 0)     AS "U_MGS_CL_EXGES",
            IFNULL(F."U_MGS_CL_EXSER", 0)     AS "U_MGS_CL_EXSER",
            IFNULL(F."U_MGS_CL_EXMAR", 0)     AS "U_MGS_CL_EXMAR",
            IFNULL(F."U_MGS_CL_PRIMARY", '')   AS "U_MGS_CL_PRIMARY"
        FROM "OPRJ" P
        LEFT JOIN (
            SELECT
                C."DocEntry",
                D."LineId",
                D."U_MGS_CL_TIENDA",
                D."U_MGS_CL_NOMTIE",
                D."U_MGS_CL_META",
                D."U_MGS_CL_RENTA",
                D."U_MGS_CL_VAN",
                D."U_MGS_CL_ESTPER",
                D."U_MGS_CL_GASADM",
                D."U_MGS_CL_COMTAR",
                D."U_MGS_CL_IMPUES",
                D."U_MGS_CL_REGALI",
                D."U_MGS_CL_AUSER",
                D."U_MGS_CL_AUOPE",
                D."U_MGS_CL_AUCCC",
                D."U_MGS_CL_AUADH",
                D."U_MGS_CL_CLIMA",
                D."U_MGS_CL_RUSTI",
                D."U_MGS_CL_MELID",
                D."U_MGS_CL_ADMGR",
                D."U_MGS_CL_EXGES",
                D."U_MGS_CL_EXSER",
                D."U_MGS_CL_EXMAR",
                D."U_MGS_CL_PRIMARY"
            FROM "@MGS_CL_FACCAB" C
            JOIN "@MGS_CL_FACDET" D
              ON D."DocEntry" = C."DocEntry"
            WHERE TO_VARCHAR(C."U_MGS_CL_PERIODO", 'YYYY-MM') = :lvPeriodoBase
        ) F ON F."U_MGS_CL_TIENDA" = P."PrjCode"
        WHERE P."Active" = 'Y'
        ORDER BY P."PrjCode", P."PrjName";


    ELSEIF vTipo = 'Get_VanTienda' THEN

        SELECT
            "PrjCode" AS "PrjCode",
            "PrjName" AS "PrjName"
        FROM "OPRJ"
        WHERE "Active" = 'Y'
        ORDER BY "PrjCode";


    ELSEIF vTipo = 'Get_VanGrupoM' THEN

        SELECT
            "Code"           AS "Code",
            "Name"           AS "Name"
        FROM "@MGS_CL_VANGRP"
        WHERE IFNULL("U_MGS_CL_ACTIVO", 'NO') = 'SI'
        ORDER BY "Code";


    ELSEIF vTipo = 'Get_VanTipo' THEN

        SELECT
            "Code"           AS "Code",
            "Name"           AS "Name"
        FROM "@MGS_CL_VANTIPO"
        WHERE IFNULL("U_MGS_CL_ACTIVO", 'NO') = 'SI'
        ORDER BY "Code";


    ELSEIF vTipo = 'Get_VanItemM' THEN

        SELECT
            "ItemCode" AS "ItemCode",
            "ItemName" AS "ItemName"
        FROM "OITM"
        WHERE "InvntItem" = 'Y'
          AND (
                :vParam1 = ''
             OR UPPER("ItemCode") LIKE '%' || UPPER(:vParam1) || '%'
             OR UPPER("ItemName") LIKE '%' || UPPER(:vParam1) || '%'
          )
        ORDER BY "ItemCode";


    ELSEIF vTipo = 'Get_VanTdaGrp' THEN

        SELECT
            D."DocEntry"        AS "DocEntry",
            D."LineId"          AS "LineId",
            D."U_MGS_CL_GRPCOD" AS "U_MGS_CL_GRPCOD",
            G."Name"            AS "U_MGS_CL_GRPNOM",
            IFNULL(D."U_MGS_CL_TIPO", '') AS "U_MGS_CL_TIPO",
            IFNULL(D."U_MGS_CL_PORC", 0) AS "U_MGS_CL_PORC"
        FROM "@MGS_CL_VANTCAB" H
        JOIN "@MGS_CL_VANTDET" D ON D."DocEntry" = H."DocEntry"
        LEFT JOIN "@MGS_CL_VANGRP" G ON G."Code" = D."U_MGS_CL_GRPCOD"
        WHERE H."U_MGS_CL_TIENDA" = :vParam1
        ORDER BY D."LineId";


    ELSEIF vTipo = 'Get_VanGrpArt' THEN

        SELECT
            D."DocEntry"          AS "DocEntry",
            D."LineId"            AS "LineId",
            D."U_MGS_CL_ITEMCOD"  AS "U_MGS_CL_ITEMCOD",
            O."ItemName"          AS "U_MGS_CL_ITEMNAM",
            IFNULL(D."U_MGS_CL_TIPO", '') AS "U_MGS_CL_TIPO",
            IFNULL(D."U_MGS_CL_PORC", 0) AS "U_MGS_CL_PORC"
        FROM "@MGS_CL_VANACAB" H
        JOIN "@MGS_CL_VANADET" D ON D."DocEntry" = H."DocEntry"
        LEFT JOIN "OITM" O ON O."ItemCode" = D."U_MGS_CL_ITEMCOD"
        WHERE H."U_MGS_CL_GRPCOD" = :vParam1
        ORDER BY D."LineId";


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
