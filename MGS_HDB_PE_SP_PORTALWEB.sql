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
          AND IFNULL(D."U_MGS_CL_ACTIVO",'NO') = 'SI'
        ORDER BY D."LineId";


    ELSEIF vTipo = 'Get_VanGrpArt' THEN

		 
        SELECT
            D."DocEntry"          AS "DocEntry",
            D."LineId"            AS "LineId",
            D."U_MGS_CL_GRPCOD"   AS "U_MGS_CL_GRPCOD",
            D."U_MGS_CL_ITEMCOD"  AS "U_MGS_CL_ITEMCOD",
            CASE
                WHEN IFNULL(D."U_MGS_CL_ITEMNAM", '') = '' THEN O."ItemName"
                ELSE D."U_MGS_CL_ITEMNAM"
            END AS "U_MGS_CL_ITEMNAM",
            IFNULL(D."U_MGS_CL_TIPO", '') AS "U_MGS_CL_TIPO",
            IFNULL(D."U_MGS_CL_PORC", 0) AS "U_MGS_CL_PORC",
            IFNULL(D."U_MGS_CL_ACTIVO", 'NO') AS "U_MGS_CL_ACTIVO"
        FROM "@MGS_CL_VANTIAD" D
        INNER JOIN "@MGS_CL_VANTCAB" H ON D."DocEntry" = H."DocEntry"
        LEFT JOIN "OITM" O ON O."ItemCode" = D."U_MGS_CL_ITEMCOD"
        WHERE H."U_MGS_CL_TIENDA" = :vParam1
          AND D."U_MGS_CL_GRPCOD" = :vParam2
          AND IFNULL(D."U_MGS_CL_ACTIVO",'NO') = 'SI'
        ORDER BY D."LineId";


    ELSEIF vTipo = 'Get_VanItemTienda' THEN

        SELECT
            D."U_MGS_CL_GRPCOD" AS "U_MGS_CL_GRPCOD",
            G."Name" AS "U_MGS_CL_GRPNOM"
        FROM "@MGS_CL_VANTIAD" D
        JOIN "@MGS_CL_VANTCAB" H ON D."DocEntry" = H."DocEntry"
        LEFT JOIN "@MGS_CL_VANGRP" G ON G."Code" = D."U_MGS_CL_GRPCOD"
        WHERE H."U_MGS_CL_TIENDA" = :vParam1
          AND D."U_MGS_CL_ITEMCOD" = :vParam2
          AND IFNULL(D."U_MGS_CL_ACTIVO",'NO') = 'SI'
          AND (:vParam3 = '' OR D."U_MGS_CL_GRPCOD" <> :vParam3)
        LIMIT 1;
            
        


    ELSEIF vTipo = 'Get_VanTdaNom' THEN

        SELECT
            "PrjName" AS "PrjName"
        FROM "OPRJ"
        WHERE "PrjCode" = :vParam1;


    ELSEIF vTipo = 'Get_VanCab' THEN

        SELECT
            "DocEntry" AS "DocEntry"
        FROM "@MGS_CL_VANTCAB"
        WHERE "U_MGS_CL_TIENDA" = :vParam1;


    ELSEIF vTipo = 'Get_VanGrpEx' THEN

        SELECT
            COUNT(1) AS "Total"
        FROM "@MGS_CL_VANTCAB" H
        JOIN "@MGS_CL_VANTDET" D ON D."DocEntry" = H."DocEntry"
        WHERE H."U_MGS_CL_TIENDA" = :vParam1
          AND D."U_MGS_CL_GRPCOD" = :vParam2
          AND IFNULL(D."U_MGS_CL_ACTIVO",'NO') = 'SI';


    ELSEIF vTipo = 'Get_VanGrpNom' THEN

        SELECT
            "Name" AS "Name"
        FROM "@MGS_CL_VANGRP"
        WHERE "Code" = :vParam1;


    ELSEIF vTipo = 'Get_ClienteMon' THEN

        SELECT
            B."CardCode" AS "CardCode",
            CASE WHEN A."U_MGS_CL_MONPRO" = '1' THEN 'USD' ELSE 'SOL' END AS "CurCode",
            A."PrjName" AS "Proyecto_Nombre",
            CASE WHEN A."Active" = 'Y' THEN 'SI' ELSE 'NO' END AS "Activo",
            A."U_MGS_CL_ESTPRO" AS "Estado",
            A."U_MGS_CL_MONPRO" AS "Moneda"
        FROM "OPRJ" A
        INNER JOIN "OCRD" B ON A."U_MGS_CL_RUCPRO" = B."LicTradNum"
        WHERE A."PrjCode" = :vParam1
          AND B."CardType" = 'C';


    ELSEIF vTipo = 'Get_OVByPrj' THEN

        IF :vParam2 = 'Y' THEN
            SELECT TOP 1
                "DocEntry" AS "DocEntry"
            FROM "RDR1"
            WHERE "Project" = :vParam1
            ORDER BY "DocEntry" DESC;
        ELSE
            SELECT TOP 1
                T0."DocEntry" AS "DocEntry"
            FROM "PRQ1" T0
            INNER JOIN "OPRQ" T1 ON T1."DocEntry" = T0."DocEntry"
            WHERE T1."CANCELED" = 'N'
              AND T1."DocStatus" = 'O'
              AND T0."Project" = :vParam1
            ORDER BY T0."DocEntry" DESC;
        END IF;


    ELSEIF vTipo = 'Get_OVByCnt' THEN

        SELECT
            "DocEntry" AS "DocEntry"
        FROM "ORDR"
        WHERE "U_MGS_CL_CODINTPYP" = :vParam1
          AND "CANCELED" = 'N';


    ELSEIF vTipo = 'Get_ValidaRuc' THEN

        IF :vParam2 = 'Projects' THEN
            SELECT TOP 1
                B."CardCode" AS "CardCode"
            FROM "OCRD" B
            WHERE B."LicTradNum" = :vParam1
              AND B."CardType" = 'C';
        ELSEIF :vParam2 = 'JournalEntries' THEN
            SELECT TOP 1
                B."AcctCode" AS "CardCode"
            FROM "OACT" B
            WHERE B."AcctCode" = :vParam1;
        END IF;


    ELSEIF vTipo = 'Get_DocCabPRQ' THEN

        SELECT
            "Cabecera"."DocNum",
            "Cabecera"."DocDate",
            IFNULL("Cabecera"."NumAtCard",'') AS "NumAtCard",
            "Cabecera"."TaxDate",
            IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') AS "Cod_PyP",
            "Cabecera"."DocEntry",
            "Cabecera"."DocDueDate",
            MAX("Detalle"."Project") AS "Project",
            IFNULL(t0."Name",'Solicitud de compra') AS "tipoDoc",
            "Cabecera"."DocCur",
            (SELECT SUM("d0"."LineTotal")
             FROM "PRQ1" "d0"
             WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalSol",
            (SELECT SUM("d0"."TotalSumSy")
             FROM "PRQ1" "d0"
             WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalDol",
            CASE WHEN "Cabecera"."DocStatus" = 'O' THEN 'ABIERTO'
                 WHEN "Cabecera"."DocStatus" = 'C' THEN 'CERRADO' END AS "Status",
            IFNULL("Cabecera"."CardCode",'') AS "Ruc",
            IFNULL("Cabecera"."CardName",'') AS "Proveedor",
            IFNULL("Cabecera"."JrnlMemo",'') AS "JrnlMemo",
            IFNULL(t1."USER_CODE",'') AS "UserCreator"
        FROM "OPRQ" "Cabecera"
        INNER JOIN "PRQ1" "Detalle" ON "Cabecera"."DocEntry" = "Detalle"."DocEntry"
        LEFT JOIN "OIDC" t0 ON "Cabecera"."Indicator" = t0."Code"
        INNER JOIN "OUSR" t1 ON "Cabecera"."UserSign" = t1."USERID"
        WHERE "Cabecera"."CANCELED" = 'N'
          AND "Cabecera"."DocType" LIKE '%'
          AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
          AND ("Detalle"."Project" <> 'GENERICO' AND "Detalle"."Project" LIKE '%' || :vParam1 || '%')
          AND "Detalle"."DocEntry" IS NOT NULL
          AND IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') <> ''
        GROUP BY
            "Cabecera"."DocNum",
            "Cabecera"."DocDate",
            "Cabecera"."NumAtCard",
            "Cabecera"."TaxDate",
            "Cabecera"."U_MGS_CL_CODINTPYP",
            "Cabecera"."DocEntry",
            "Cabecera"."DocDueDate",
            t0."Name",
            "Cabecera"."DocCur",
            "Cabecera"."DocStatus",
            "Cabecera"."CardCode",
            "Cabecera"."CardName",
            "Cabecera"."JrnlMemo",
            t1."USER_CODE";


    ELSEIF vTipo = 'Get_DocCabPCH' THEN

        IF :vParam2 = '1' THEN
            SELECT
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                IFNULL("Cabecera"."NumAtCard",'') AS "NumAtCard",
                "Cabecera"."TaxDate",
                IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') AS "Cod_PyP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                MAX("Detalle"."Project") AS "Project",
                IFNULL(t0."Name",'Solicitud de compra') AS "tipoDoc",
                "Cabecera"."DocCur",
                (SELECT SUM("d0"."LineTotal")
                 FROM "PCH1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalSol",
                (SELECT SUM("d0"."TotalSumSy")
                 FROM "PCH1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalDol",
                CASE WHEN "Cabecera"."DocStatus" = 'O' THEN 'ABIERTO'
                     WHEN "Cabecera"."DocStatus" = 'C' THEN 'CERRADO' END AS "Status",
                IFNULL("Cabecera"."CardCode",'') AS "Ruc",
                IFNULL("Cabecera"."CardName",'') AS "Proveedor",
                IFNULL("Cabecera"."JrnlMemo",'') AS "JrnlMemo",
                IFNULL(t1."USER_CODE",'') AS "UserCreator"
            FROM "OPCH" "Cabecera"
            INNER JOIN "PCH1" "Detalle" ON "Cabecera"."DocEntry" = "Detalle"."DocEntry"
            LEFT JOIN "OIDC" t0 ON "Cabecera"."Indicator" = t0."Code"
            INNER JOIN "OUSR" t1 ON "Cabecera"."UserSign" = t1."USERID"
            WHERE "Cabecera"."CANCELED" = 'N'
              AND "Cabecera"."DocType" LIKE '%'
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND ("Detalle"."Project" <> 'GENERICO' AND "Detalle"."Project" LIKE '%' || :vParam1 || '%')
              AND "Detalle"."DocEntry" IS NOT NULL
            GROUP BY
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                "Cabecera"."NumAtCard",
                "Cabecera"."TaxDate",
                "Cabecera"."U_MGS_CL_CODINTPYP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                t0."Name",
                "Cabecera"."DocCur",
                "Cabecera"."DocStatus",
                "Cabecera"."CardCode",
                "Cabecera"."CardName",
                "Cabecera"."JrnlMemo",
                t1."USER_CODE"
            UNION
            SELECT
                MAX("CABHIST"."Code") AS "DocNum",
                MAX("CABHIST"."U_MGS_CL_FECCON") AS "DocDate",
                "CABHIST"."U_MGS_CL_NRODOC" AS "NumAtCard",
                MAX("CABHIST"."U_MGS_CL_FECDOC") AS "TaxDate",
                MAX("CABHIST"."U_MGS_CL_CODPRO") AS "Cod_PyP",
                '0' AS "DocEntry",
                MAX("CABHIST"."U_MGS_CL_FECDOC") AS "DocDueDate",
                MAX("CABHIST"."U_MGS_CL_CODPRY") AS "Project",
                t0."Name" AS "tipoDoc",
                "CABHIST"."U_MGS_CL_MONEDA" AS "DocCur",
                (SELECT SUM("ch"."U_MGS_CL_COSTOT")
                 FROM "@MGS_CL_HISCOM" "ch"
                 WHERE "ch"."U_MGS_CL_CODPRO" = MAX("CABHIST"."U_MGS_CL_CODPRO")
                   AND "ch"."U_MGS_CL_NRODOC" = "CABHIST"."U_MGS_CL_NRODOC") AS "TotalSol",
                CAST('0' AS DECIMAL(18,6)) AS "TotalDol",
                '' AS "Status",
                MAX("CABHIST"."U_MGS_CL_RUCDNI") AS "Ruc",
                MAX("CABHIST"."U_MGS_CL_PROVEE") AS "Proveedor",
                'INFORMACION DEL HISTORICO' AS "JrnlMemo",
                '' AS "UserCreator"
            FROM "@MGS_CL_HISCOM" "CABHIST"
            INNER JOIN "OIDC" t0 ON t0."Code" = "CABHIST"."U_MGS_CL_TIPDOC"
            WHERE "CABHIST"."U_MGS_CL_CODPRY" LIKE '%' || :vParam1 || '%'
            GROUP BY
                "CABHIST"."U_MGS_CL_NRODOC",
                t0."Name",
                "CABHIST"."U_MGS_CL_MONEDA";
        ELSE
            SELECT
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                IFNULL("Cabecera"."NumAtCard",'') AS "NumAtCard",
                "Cabecera"."TaxDate",
                IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') AS "Cod_PyP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                MAX("Detalle"."Project") AS "Project",
                IFNULL(t0."Name",'Solicitud de compra') AS "tipoDoc",
                "Cabecera"."DocCur",
                (SELECT SUM("d0"."LineTotal")
                 FROM "PCH1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalSol",
                (SELECT SUM("d0"."TotalSumSy")
                 FROM "PCH1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalDol",
                CASE WHEN "Cabecera"."DocStatus" = 'O' THEN 'ABIERTO'
                     WHEN "Cabecera"."DocStatus" = 'C' THEN 'CERRADO' END AS "Status",
                IFNULL("Cabecera"."CardCode",'') AS "Ruc",
                IFNULL("Cabecera"."CardName",'') AS "Proveedor",
                IFNULL("Cabecera"."JrnlMemo",'') AS "JrnlMemo",
                IFNULL(t1."USER_CODE",'') AS "UserCreator"
            FROM "OPCH" "Cabecera"
            INNER JOIN "PCH1" "Detalle" ON "Cabecera"."DocEntry" = "Detalle"."DocEntry"
            LEFT JOIN "OIDC" t0 ON "Cabecera"."Indicator" = t0."Code"
            INNER JOIN "OUSR" t1 ON "Cabecera"."UserSign" = t1."USERID"
            WHERE "Cabecera"."CANCELED" = 'N'
              AND "Cabecera"."DocType" LIKE '%'
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND ("Detalle"."Project" <> 'GENERICO' AND "Detalle"."Project" LIKE '%' || :vParam1 || '%')
              AND "Detalle"."DocEntry" IS NOT NULL
            GROUP BY
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                "Cabecera"."NumAtCard",
                "Cabecera"."TaxDate",
                "Cabecera"."U_MGS_CL_CODINTPYP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                t0."Name",
                "Cabecera"."DocCur",
                "Cabecera"."DocStatus",
                "Cabecera"."CardCode",
                "Cabecera"."CardName",
                "Cabecera"."JrnlMemo",
                t1."USER_CODE";
        END IF;


    ELSEIF vTipo = 'Get_DocCabRPC' THEN

        IF :vParam2 = '1' THEN
            SELECT
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                IFNULL("Cabecera"."NumAtCard",'') AS "NumAtCard",
                "Cabecera"."TaxDate",
                IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') AS "Cod_PyP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                MAX("Detalle"."Project") AS "Project",
                IFNULL(t0."Name",'Solicitud de compra') AS "tipoDoc",
                "Cabecera"."DocCur",
                (SELECT SUM("d0"."LineTotal")
                 FROM "RPC1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalSol",
                (SELECT SUM("d0"."TotalSumSy")
                 FROM "RPC1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalDol",
                CASE WHEN "Cabecera"."DocStatus" = 'O' THEN 'ABIERTO'
                     WHEN "Cabecera"."DocStatus" = 'C' THEN 'CERRADO' END AS "Status",
                IFNULL("Cabecera"."CardCode",'') AS "Ruc",
                IFNULL("Cabecera"."CardName",'') AS "Proveedor",
                IFNULL("Cabecera"."JrnlMemo",'') AS "JrnlMemo",
                IFNULL(t1."USER_CODE",'') AS "UserCreator"
            FROM "ORPC" "Cabecera"
            INNER JOIN "RPC1" "Detalle" ON "Cabecera"."DocEntry" = "Detalle"."DocEntry"
            LEFT JOIN "OIDC" t0 ON "Cabecera"."Indicator" = t0."Code"
            INNER JOIN "OUSR" t1 ON "Cabecera"."UserSign" = t1."USERID"
            WHERE "Cabecera"."CANCELED" = 'N'
              AND "Cabecera"."DocType" LIKE '%'
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND ("Detalle"."Project" <> 'GENERICO' AND "Detalle"."Project" LIKE '%' || :vParam1 || '%')
              AND "Detalle"."DocEntry" IS NOT NULL
              AND IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') <> ''
            GROUP BY
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                "Cabecera"."NumAtCard",
                "Cabecera"."TaxDate",
                "Cabecera"."U_MGS_CL_CODINTPYP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                t0."Name",
                "Cabecera"."DocCur",
                "Cabecera"."DocStatus",
                "Cabecera"."CardCode",
                "Cabecera"."CardName",
                "Cabecera"."JrnlMemo",
                t1."USER_CODE"
            UNION
            SELECT
                MAX("CABHIST"."Code") AS "DocNum",
                MAX("CABHIST"."U_MGS_CL_FECCON") AS "DocDate",
                "CABHIST"."U_MGS_CL_NRODOC" AS "NumAtCard",
                MAX("CABHIST"."U_MGS_CL_FECDOC") AS "TaxDate",
                MAX("CABHIST"."U_MGS_CL_CODPRO") AS "Cod_PyP",
                '0' AS "DocEntry",
                MAX("CABHIST"."U_MGS_CL_FECDOC") AS "DocDueDate",
                MAX("CABHIST"."U_MGS_CL_CODPRY") AS "Project",
                t0."Name" AS "tipoDoc",
                "CABHIST"."U_MGS_CL_MONEDA" AS "DocCur",
                (SELECT SUM("ch"."U_MGS_CL_COSTOT")
                 FROM "@MGS_CL_HISCOM" "ch"
                 WHERE "ch"."U_MGS_CL_CODPRO" = MAX("CABHIST"."U_MGS_CL_CODPRO")
                   AND "ch"."U_MGS_CL_NRODOC" = "CABHIST"."U_MGS_CL_NRODOC") AS "TotalSol",
                CAST('0' AS DECIMAL(18,6)) AS "TotalDol",
                '' AS "Status",
                MAX("CABHIST"."U_MGS_CL_RUCDNI") AS "Ruc",
                MAX("CABHIST"."U_MGS_CL_PROVEE") AS "Proveedor",
                'INFORMACION DEL HISTORICO' AS "JrnlMemo",
                '' AS "UserCreator"
            FROM "@MGS_CL_HISCOM" "CABHIST"
            INNER JOIN "OIDC" t0 ON t0."Code" = "CABHIST"."U_MGS_CL_TIPDOC"
            WHERE "CABHIST"."U_MGS_CL_CODPRY" LIKE '%' || :vParam1 || '%'
            GROUP BY
                "CABHIST"."U_MGS_CL_NRODOC",
                t0."Name",
                "CABHIST"."U_MGS_CL_MONEDA";
        ELSE
            SELECT
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                IFNULL("Cabecera"."NumAtCard",'') AS "NumAtCard",
                "Cabecera"."TaxDate",
                IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') AS "Cod_PyP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                MAX("Detalle"."Project") AS "Project",
                IFNULL(t0."Name",'Solicitud de compra') AS "tipoDoc",
                "Cabecera"."DocCur",
                (SELECT SUM("d0"."LineTotal")
                 FROM "RPC1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalSol",
                (SELECT SUM("d0"."TotalSumSy")
                 FROM "RPC1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalDol",
                CASE WHEN "Cabecera"."DocStatus" = 'O' THEN 'ABIERTO'
                     WHEN "Cabecera"."DocStatus" = 'C' THEN 'CERRADO' END AS "Status",
                IFNULL("Cabecera"."CardCode",'') AS "Ruc",
                IFNULL("Cabecera"."CardName",'') AS "Proveedor",
                IFNULL("Cabecera"."JrnlMemo",'') AS "JrnlMemo",
                IFNULL(t1."USER_CODE",'') AS "UserCreator"
            FROM "ORPC" "Cabecera"
            INNER JOIN "RPC1" "Detalle" ON "Cabecera"."DocEntry" = "Detalle"."DocEntry"
            LEFT JOIN "OIDC" t0 ON "Cabecera"."Indicator" = t0."Code"
            INNER JOIN "OUSR" t1 ON "Cabecera"."UserSign" = t1."USERID"
            WHERE "Cabecera"."CANCELED" = 'N'
              AND "Cabecera"."DocType" LIKE '%'
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND ("Detalle"."Project" <> 'GENERICO' AND "Detalle"."Project" LIKE '%' || :vParam1 || '%')
              AND "Detalle"."DocEntry" IS NOT NULL
              AND IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') <> ''
            GROUP BY
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                "Cabecera"."NumAtCard",
                "Cabecera"."TaxDate",
                "Cabecera"."U_MGS_CL_CODINTPYP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                t0."Name",
                "Cabecera"."DocCur",
                "Cabecera"."DocStatus",
                "Cabecera"."CardCode",
                "Cabecera"."CardName",
                "Cabecera"."JrnlMemo",
                t1."USER_CODE";
        END IF;


    ELSEIF vTipo = 'Get_DocCabINV' THEN

        IF :vParam2 = '1' THEN
            SELECT
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                IFNULL("Cabecera"."NumAtCard",'') AS "NumAtCard",
                "Cabecera"."TaxDate",
                IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') AS "Cod_PyP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                MAX("Detalle"."Project") AS "Project",
                IFNULL(t0."Name",'Solicitud de compra') AS "tipoDoc",
                "Cabecera"."DocCur",
                (SELECT SUM("d0"."LineTotal")
                 FROM "INV1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalSol",
                (SELECT SUM("d0"."TotalSumSy")
                 FROM "INV1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalDol",
                CASE WHEN "Cabecera"."DocStatus" = 'O' THEN 'ABIERTO'
                     WHEN "Cabecera"."DocStatus" = 'C' THEN 'CERRADO' END AS "Status",
                IFNULL("Cabecera"."CardCode",'') AS "Ruc",
                IFNULL("Cabecera"."CardName",'') AS "Proveedor",
                IFNULL("Cabecera"."JrnlMemo",'') AS "JrnlMemo",
                IFNULL(t1."USER_CODE",'') AS "UserCreator"
            FROM "OINV" "Cabecera"
            INNER JOIN "INV1" "Detalle" ON "Cabecera"."DocEntry" = "Detalle"."DocEntry"
            LEFT JOIN "OIDC" t0 ON "Cabecera"."Indicator" = t0."Code"
            INNER JOIN "OUSR" t1 ON "Cabecera"."UserSign" = t1."USERID"
            WHERE "Cabecera"."CANCELED" = 'N'
              AND "Cabecera"."DocType" LIKE 'I'
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND ("Detalle"."Project" <> 'GENERICO' AND "Detalle"."Project" LIKE '%' || :vParam1 || '%')
              AND "Detalle"."DocEntry" IS NOT NULL
            GROUP BY
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                "Cabecera"."NumAtCard",
                "Cabecera"."TaxDate",
                "Cabecera"."U_MGS_CL_CODINTPYP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                t0."Name",
                "Cabecera"."DocCur",
                "Cabecera"."DocStatus",
                "Cabecera"."CardCode",
                "Cabecera"."CardName",
                "Cabecera"."JrnlMemo",
                t1."USER_CODE"
            UNION
            SELECT
                MAX("CABHIST"."Code") AS "DocNum",
                MAX("CABHIST"."U_MGS_CL_FECCON") AS "DocDate",
                "CABHIST"."U_MGS_CL_NRODOC" AS "NumAtCard",
                MAX("CABHIST"."U_MGS_CL_FECDOC") AS "TaxDate",
                MAX("CABHIST"."U_MGS_CL_CODPRO") AS "Cod_PyP",
                '0' AS "DocEntry",
                MAX("CABHIST"."U_MGS_CL_FECDOC") AS "DocDueDate",
                MAX("CABHIST"."U_MGS_CL_CODPRY") AS "Project",
                t0."Name" AS "tipoDoc",
                "CABHIST"."U_MGS_CL_MONEDA" AS "DocCur",
                (SELECT SUM("ch"."U_MGS_CL_COSTOT")
                 FROM "@MGS_CL_HISVEN" "ch"
                 WHERE "ch"."U_MGS_CL_CODPRO" = MAX("CABHIST"."U_MGS_CL_CODPRO")
                   AND "ch"."U_MGS_CL_NRODOC" = "CABHIST"."U_MGS_CL_NRODOC") AS "TotalSol",
                (SELECT SUM("ch"."U_MGS_CL_COTUSD")
                 FROM "@MGS_CL_HISVEN" "ch"
                 WHERE "ch"."U_MGS_CL_CODPRO" = MAX("CABHIST"."U_MGS_CL_CODPRO")
                   AND "ch"."U_MGS_CL_NRODOC" = "CABHIST"."U_MGS_CL_NRODOC") AS "TotalDol",
                '' AS "Status",
                MAX("CABHIST"."U_MGS_CL_RUCDNI") AS "Ruc",
                MAX("CABHIST"."U_MGS_CL_PROVEE") AS "Proveedor",
                'INFORMACION DEL HISTORICO' AS "JrnlMemo",
                '' AS "UserCreator"
            FROM "@MGS_CL_HISVEN" "CABHIST"
            INNER JOIN "OIDC" t0 ON t0."Code" = "CABHIST"."U_MGS_CL_TIPDOC"
            WHERE "CABHIST"."U_MGS_CL_CODPRY" LIKE '%' || :vParam1 || '%'
            GROUP BY
                "CABHIST"."U_MGS_CL_NRODOC",
                t0."Name",
                "CABHIST"."U_MGS_CL_MONEDA";
        ELSE
            SELECT
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                IFNULL("Cabecera"."NumAtCard",'') AS "NumAtCard",
                "Cabecera"."TaxDate",
                IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') AS "Cod_PyP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                MAX("Detalle"."Project") AS "Project",
                IFNULL(t0."Name",'Solicitud de compra') AS "tipoDoc",
                "Cabecera"."DocCur",
                (SELECT SUM("d0"."LineTotal")
                 FROM "INV1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalSol",
                (SELECT SUM("d0"."TotalSumSy")
                 FROM "INV1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalDol",
                CASE WHEN "Cabecera"."DocStatus" = 'O' THEN 'ABIERTO'
                     WHEN "Cabecera"."DocStatus" = 'C' THEN 'CERRADO' END AS "Status",
                IFNULL("Cabecera"."CardCode",'') AS "Ruc",
                IFNULL("Cabecera"."CardName",'') AS "Proveedor",
                IFNULL("Cabecera"."JrnlMemo",'') AS "JrnlMemo",
                IFNULL(t1."USER_CODE",'') AS "UserCreator"
            FROM "OINV" "Cabecera"
            INNER JOIN "INV1" "Detalle" ON "Cabecera"."DocEntry" = "Detalle"."DocEntry"
            LEFT JOIN "OIDC" t0 ON "Cabecera"."Indicator" = t0."Code"
            INNER JOIN "OUSR" t1 ON "Cabecera"."UserSign" = t1."USERID"
            WHERE "Cabecera"."CANCELED" = 'N'
              AND "Cabecera"."DocType" LIKE 'I'
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND ("Detalle"."Project" <> 'GENERICO' AND "Detalle"."Project" LIKE '%' || :vParam1 || '%')
              AND "Detalle"."DocEntry" IS NOT NULL
            GROUP BY
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                "Cabecera"."NumAtCard",
                "Cabecera"."TaxDate",
                "Cabecera"."U_MGS_CL_CODINTPYP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                t0."Name",
                "Cabecera"."DocCur",
                "Cabecera"."DocStatus",
                "Cabecera"."CardCode",
                "Cabecera"."CardName",
                "Cabecera"."JrnlMemo",
                t1."USER_CODE";
        END IF;


    ELSEIF vTipo = 'Get_DocCabRIN' THEN

        IF :vParam2 = '1' THEN
            SELECT
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                IFNULL("Cabecera"."NumAtCard",'') AS "NumAtCard",
                "Cabecera"."TaxDate",
                IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') AS "Cod_PyP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                MAX("Detalle"."Project") AS "Project",
                IFNULL(t0."Name",'Solicitud de compra') AS "tipoDoc",
                "Cabecera"."DocCur",
                (SELECT SUM("d0"."LineTotal")
                 FROM "RIN1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalSol",
                (SELECT SUM("d0"."TotalSumSy")
                 FROM "RIN1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalDol",
                CASE WHEN "Cabecera"."DocStatus" = 'O' THEN 'ABIERTO'
                     WHEN "Cabecera"."DocStatus" = 'C' THEN 'CERRADO' END AS "Status",
                IFNULL("Cabecera"."CardCode",'') AS "Ruc",
                IFNULL("Cabecera"."CardName",'') AS "Proveedor",
                IFNULL("Cabecera"."JrnlMemo",'') AS "JrnlMemo",
                IFNULL(t1."USER_CODE",'') AS "UserCreator"
            FROM "ORIN" "Cabecera"
            INNER JOIN "RIN1" "Detalle" ON "Cabecera"."DocEntry" = "Detalle"."DocEntry"
            LEFT JOIN "OIDC" t0 ON "Cabecera"."Indicator" = t0."Code"
            INNER JOIN "OUSR" t1 ON "Cabecera"."UserSign" = t1."USERID"
            WHERE "Cabecera"."CANCELED" = 'N'
              AND "Cabecera"."DocType" LIKE 'I'
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND ("Detalle"."Project" <> 'GENERICO' AND "Detalle"."Project" LIKE '%' || :vParam1 || '%')
              AND "Detalle"."DocEntry" IS NOT NULL
              AND IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') <> ''
            GROUP BY
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                "Cabecera"."NumAtCard",
                "Cabecera"."TaxDate",
                "Cabecera"."U_MGS_CL_CODINTPYP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                t0."Name",
                "Cabecera"."DocCur",
                "Cabecera"."DocStatus",
                "Cabecera"."CardCode",
                "Cabecera"."CardName",
                "Cabecera"."JrnlMemo",
                t1."USER_CODE"
            UNION
            SELECT
                MAX("CABHIST"."Code") AS "DocNum",
                MAX("CABHIST"."U_MGS_CL_FECCON") AS "DocDate",
                "CABHIST"."U_MGS_CL_NRODOC" AS "NumAtCard",
                MAX("CABHIST"."U_MGS_CL_FECDOC") AS "TaxDate",
                MAX("CABHIST"."U_MGS_CL_CODPRO") AS "Cod_PyP",
                '0' AS "DocEntry",
                MAX("CABHIST"."U_MGS_CL_FECDOC") AS "DocDueDate",
                MAX("CABHIST"."U_MGS_CL_CODPRY") AS "Project",
                t0."Name" AS "tipoDoc",
                "CABHIST"."U_MGS_CL_MONEDA" AS "DocCur",
                (SELECT SUM("ch"."U_MGS_CL_COSTOT")
                 FROM "@MGS_CL_HISVEN" "ch"
                 WHERE "ch"."U_MGS_CL_CODPRO" = MAX("CABHIST"."U_MGS_CL_CODPRO")
                   AND "ch"."U_MGS_CL_NRODOC" = "CABHIST"."U_MGS_CL_NRODOC") AS "TotalSol",
                (SELECT SUM("ch"."U_MGS_CL_COTUSD")
                 FROM "@MGS_CL_HISVEN" "ch"
                 WHERE "ch"."U_MGS_CL_CODPRO" = MAX("CABHIST"."U_MGS_CL_CODPRO")
                   AND "ch"."U_MGS_CL_NRODOC" = "CABHIST"."U_MGS_CL_NRODOC") AS "TotalDol",
                '' AS "Status",
                MAX("CABHIST"."U_MGS_CL_RUCDNI") AS "Ruc",
                MAX("CABHIST"."U_MGS_CL_PROVEE") AS "Proveedor",
                'INFORMACION DEL HISTORICO' AS "JrnlMemo",
                '' AS "UserCreator"
            FROM "@MGS_CL_HISVEN" "CABHIST"
            INNER JOIN "OIDC" t0 ON t0."Code" = "CABHIST"."U_MGS_CL_TIPDOC"
            WHERE "CABHIST"."U_MGS_CL_CODPRY" LIKE '%' || :vParam1 || '%'
            GROUP BY
                "CABHIST"."U_MGS_CL_NRODOC",
                t0."Name",
                "CABHIST"."U_MGS_CL_MONEDA";
        ELSE
            SELECT
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                IFNULL("Cabecera"."NumAtCard",'') AS "NumAtCard",
                "Cabecera"."TaxDate",
                IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') AS "Cod_PyP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                MAX("Detalle"."Project") AS "Project",
                IFNULL(t0."Name",'Solicitud de compra') AS "tipoDoc",
                "Cabecera"."DocCur",
                (SELECT SUM("d0"."LineTotal")
                 FROM "RIN1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalSol",
                (SELECT SUM("d0"."TotalSumSy")
                 FROM "RIN1" "d0"
                 WHERE "d0"."DocEntry" = "Cabecera"."DocEntry") AS "TotalDol",
                CASE WHEN "Cabecera"."DocStatus" = 'O' THEN 'ABIERTO'
                     WHEN "Cabecera"."DocStatus" = 'C' THEN 'CERRADO' END AS "Status",
                IFNULL("Cabecera"."CardCode",'') AS "Ruc",
                IFNULL("Cabecera"."CardName",'') AS "Proveedor",
                IFNULL("Cabecera"."JrnlMemo",'') AS "JrnlMemo",
                IFNULL(t1."USER_CODE",'') AS "UserCreator"
            FROM "ORIN" "Cabecera"
            INNER JOIN "RIN1" "Detalle" ON "Cabecera"."DocEntry" = "Detalle"."DocEntry"
            LEFT JOIN "OIDC" t0 ON "Cabecera"."Indicator" = t0."Code"
            INNER JOIN "OUSR" t1 ON "Cabecera"."UserSign" = t1."USERID"
            WHERE "Cabecera"."CANCELED" = 'N'
              AND "Cabecera"."DocType" LIKE 'I'
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND ("Detalle"."Project" <> 'GENERICO' AND "Detalle"."Project" LIKE '%' || :vParam1 || '%')
              AND "Detalle"."DocEntry" IS NOT NULL
              AND IFNULL("Cabecera"."U_MGS_CL_CODINTPYP",'') <> ''
            GROUP BY
                "Cabecera"."DocNum",
                "Cabecera"."DocDate",
                "Cabecera"."NumAtCard",
                "Cabecera"."TaxDate",
                "Cabecera"."U_MGS_CL_CODINTPYP",
                "Cabecera"."DocEntry",
                "Cabecera"."DocDueDate",
                t0."Name",
                "Cabecera"."DocCur",
                "Cabecera"."DocStatus",
                "Cabecera"."CardCode",
                "Cabecera"."CardName",
                "Cabecera"."JrnlMemo",
                t1."USER_CODE";
        END IF;


    ELSEIF vTipo = 'Get_DocDetPRQ' THEN

        SELECT
            IFNULL("Detalle"."ItemCode", "Detalle"."U_MGS_LC_SERCOM") AS "ItemCode",
            IFNULL("Detalle"."U_MGS_CL_NITEMPYP",'') AS "U_MGS_CL_NITEMPYP",
            "Detalle"."Quantity",
            "Detalle"."Price",
            IFNULL("Detalle"."U_MGS_CL_CANINI", 0) AS "cantidad_Inicial",
            IFNULL("Detalle"."U_MGS_CL_PREINI", 0) AS "costo_Unit_Inicial",
            "Detalle"."Project",
            IFNULL("Detalle"."U_MGS_CL_TIPBENPRO",'') AS "U_MGS_CL_TIPBENPRO",
            ("Detalle"."Quantity" * "Detalle"."Price") AS "LineTotal",
            "Detalle"."Quantity" * 100 AS "Porcentaje",
            IFNULL(t2."Name",'') AS "UnidadNegocio",
            IFNULL(t1."U_MGS_CL_JEFE",'') AS "JefeCuenta",
            IFNULL(t4."Name",'') AS "Familia",
            IFNULL(t3."Name",'') AS "EstadoProyecto"
        FROM "PRQ1" "Detalle"
        LEFT JOIN "OPRJ" t1 ON "Detalle"."Project" = t1."PrjCode"
        LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
        LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
        LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
        WHERE "Detalle"."DocEntry" = :vParam1
          AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
          AND "Detalle"."Project" LIKE '%' || :vParam2 || '%'
        ORDER BY "Project" ASC;


    ELSEIF vTipo = 'Get_DocDetPCH' THEN

        IF :vParam3 = '1' THEN
            SELECT
                IFNULL("Detalle"."ItemCode", "Detalle"."U_MGS_LC_SERCOM") AS "ItemCode",
                IFNULL("Detalle"."U_MGS_CL_NITEMPYP",'') AS "U_MGS_CL_NITEMPYP",
                "Detalle"."Quantity",
                "Detalle"."Price",
                IFNULL("Detalle"."U_MGS_CL_CANINI", 0) AS "cantidad_Inicial",
                IFNULL("Detalle"."U_MGS_CL_PREINI", 0) AS "costo_Unit_Inicial",
                "Detalle"."Project",
                IFNULL("Detalle"."U_MGS_CL_TIPBENPRO",'') AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."Quantity" * "Detalle"."Price") AS "LineTotal",
                "Detalle"."Quantity" * 100 AS "Porcentaje",
                IFNULL(t2."Name",'') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE",'') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name",'') AS "EstadoProyecto"
            FROM "PCH1" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."Project" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."DocEntry" = :vParam1
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND "Detalle"."Project" LIKE '%' || :vParam2 || '%'
            UNION ALL
            SELECT
                "Detalle"."U_MGS_CL_ARTIID" AS "ItemCode",
                '' AS "U_MGS_CL_NITEMPYP",
                "Detalle"."U_MGS_CL_CANTID" AS "Quantity",
                "Detalle"."U_MGS_CL_COSUNI" AS "Price",
                0 AS "cantidad_Inicial",
                0 AS "costo_Unit_Inicial",
                IFNULL("Detalle"."U_MGS_CL_CODPRY", '') AS "Project",
                '' AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."U_MGS_CL_COSTOT") AS "LineTotal",
                0 AS "Porcentaje",
                IFNULL(t2."Name", '') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE", '') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name", '') AS "EstadoProyecto"
            FROM "@MGS_CL_HISCOM" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."U_MGS_CL_CODPRO" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."U_MGS_CL_NRODOC" = :vParam4
              AND "Detalle"."U_MGS_CL_CODPRY" LIKE '%' || :vParam2 || '%'
            ORDER BY "Project" ASC;
        ELSE
            SELECT
                IFNULL("Detalle"."ItemCode", "Detalle"."U_MGS_LC_SERCOM") AS "ItemCode",
                IFNULL("Detalle"."U_MGS_CL_NITEMPYP",'') AS "U_MGS_CL_NITEMPYP",
                "Detalle"."Quantity",
                "Detalle"."Price",
                IFNULL("Detalle"."U_MGS_CL_CANINI", 0) AS "cantidad_Inicial",
                IFNULL("Detalle"."U_MGS_CL_PREINI", 0) AS "costo_Unit_Inicial",
                "Detalle"."Project",
                IFNULL("Detalle"."U_MGS_CL_TIPBENPRO",'') AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."Quantity" * "Detalle"."Price") AS "LineTotal",
                "Detalle"."Quantity" * 100 AS "Porcentaje",
                IFNULL(t2."Name",'') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE",'') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name",'') AS "EstadoProyecto"
            FROM "PCH1" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."Project" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."DocEntry" = :vParam1
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND "Detalle"."Project" LIKE '%' || :vParam2 || '%'
            ORDER BY "Project" ASC;
        END IF;


    ELSEIF vTipo = 'Get_DocDetRPC' THEN

        IF :vParam3 = '1' THEN
            SELECT
                IFNULL("Detalle"."ItemCode", "Detalle"."U_MGS_LC_SERCOM") AS "ItemCode",
                IFNULL("Detalle"."U_MGS_CL_NITEMPYP",'') AS "U_MGS_CL_NITEMPYP",
                "Detalle"."Quantity",
                "Detalle"."Price",
                IFNULL("Detalle"."U_MGS_CL_CANINI", 0) AS "cantidad_Inicial",
                IFNULL("Detalle"."U_MGS_CL_PREINI", 0) AS "costo_Unit_Inicial",
                "Detalle"."Project",
                IFNULL("Detalle"."U_MGS_CL_TIPBENPRO",'') AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."Quantity" * "Detalle"."Price") AS "LineTotal",
                "Detalle"."Quantity" * 100 AS "Porcentaje",
                IFNULL(t2."Name",'') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE",'') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name",'') AS "EstadoProyecto"
            FROM "RPC1" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."Project" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."DocEntry" = :vParam1
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND "Detalle"."Project" LIKE '%' || :vParam2 || '%'
            UNION ALL
            SELECT
                "Detalle"."U_MGS_CL_ARTIID" AS "ItemCode",
                '' AS "U_MGS_CL_NITEMPYP",
                "Detalle"."U_MGS_CL_CANTID" AS "Quantity",
                "Detalle"."U_MGS_CL_COSUNI" AS "Price",
                0 AS "cantidad_Inicial",
                0 AS "costo_Unit_Inicial",
                IFNULL("Detalle"."U_MGS_CL_CODPRY", '') AS "Project",
                '' AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."U_MGS_CL_COSTOT") AS "LineTotal",
                0 AS "Porcentaje",
                IFNULL(t2."Name", '') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE", '') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name", '') AS "EstadoProyecto"
            FROM "@MGS_CL_HISCOM" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."U_MGS_CL_CODPRO" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."U_MGS_CL_NRODOC" = :vParam4
              AND "Detalle"."U_MGS_CL_CODPRY" LIKE '%' || :vParam2 || '%'
            ORDER BY "Project" ASC;
        ELSE
            SELECT
                IFNULL("Detalle"."ItemCode", "Detalle"."U_MGS_LC_SERCOM") AS "ItemCode",
                IFNULL("Detalle"."U_MGS_CL_NITEMPYP",'') AS "U_MGS_CL_NITEMPYP",
                "Detalle"."Quantity",
                "Detalle"."Price",
                IFNULL("Detalle"."U_MGS_CL_CANINI", 0) AS "cantidad_Inicial",
                IFNULL("Detalle"."U_MGS_CL_PREINI", 0) AS "costo_Unit_Inicial",
                "Detalle"."Project",
                IFNULL("Detalle"."U_MGS_CL_TIPBENPRO",'') AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."Quantity" * "Detalle"."Price") AS "LineTotal",
                "Detalle"."Quantity" * 100 AS "Porcentaje",
                IFNULL(t2."Name",'') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE",'') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name",'') AS "EstadoProyecto"
            FROM "RPC1" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."Project" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."DocEntry" = :vParam1
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND "Detalle"."Project" LIKE '%' || :vParam2 || '%'
            ORDER BY "Project" ASC;
        END IF;


    ELSEIF vTipo = 'Get_DocDetINV' THEN

        IF :vParam3 = '1' THEN
            SELECT
                IFNULL("Detalle"."ItemCode", "Detalle"."U_MGS_LC_SERCOM") AS "ItemCode",
                IFNULL("Detalle"."U_MGS_CL_NITEMPYP",'') AS "U_MGS_CL_NITEMPYP",
                "Detalle"."Quantity",
                "Detalle"."Price",
                IFNULL("Detalle"."U_MGS_CL_CANINI", 0) AS "cantidad_Inicial",
                IFNULL("Detalle"."U_MGS_CL_PREINI", 0) AS "costo_Unit_Inicial",
                "Detalle"."Project",
                IFNULL("Detalle"."U_MGS_CL_TIPBENPRO",'') AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."Quantity" * "Detalle"."Price") AS "LineTotal",
                "Detalle"."Quantity" * 100 AS "Porcentaje",
                IFNULL(t2."Name",'') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE",'') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name",'') AS "EstadoProyecto"
            FROM "INV1" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."Project" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."DocEntry" = :vParam1
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND "Detalle"."Project" LIKE '%' || :vParam2 || '%'
            UNION ALL
            SELECT
                "Detalle"."U_MGS_CL_TARIID" AS "ItemCode",
                '' AS "U_MGS_CL_NITEMPYP",
                "Detalle"."U_MGS_CL_CANTID" AS "Quantity",
                "Detalle"."U_MGS_CL_COSUNI" AS "Price",
                0 AS "cantidad_Inicial",
                0 AS "costo_Unit_Inicial",
                IFNULL("Detalle"."U_MGS_CL_CODPRY", '') AS "Project",
                '' AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."U_MGS_CL_COSTOT") AS "LineTotal",
                0 AS "Porcentaje",
                IFNULL(t2."Name", '') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE", '') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name", '') AS "EstadoProyecto"
            FROM "@MGS_CL_HISVEN" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."U_MGS_CL_CODPRO" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."U_MGS_CL_NRODOC" = :vParam4
              AND "Detalle"."U_MGS_CL_CODPRY" LIKE '%' || :vParam2 || '%'
            ORDER BY "Project" ASC;
        ELSE
            SELECT
                IFNULL("Detalle"."ItemCode", "Detalle"."U_MGS_LC_SERCOM") AS "ItemCode",
                IFNULL("Detalle"."U_MGS_CL_NITEMPYP",'') AS "U_MGS_CL_NITEMPYP",
                "Detalle"."Quantity",
                "Detalle"."Price",
                IFNULL("Detalle"."U_MGS_CL_CANINI", 0) AS "cantidad_Inicial",
                IFNULL("Detalle"."U_MGS_CL_PREINI", 0) AS "costo_Unit_Inicial",
                "Detalle"."Project",
                IFNULL("Detalle"."U_MGS_CL_TIPBENPRO",'') AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."Quantity" * "Detalle"."Price") AS "LineTotal",
                "Detalle"."Quantity" * 100 AS "Porcentaje",
                IFNULL(t2."Name",'') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE",'') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name",'') AS "EstadoProyecto"
            FROM "INV1" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."Project" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."DocEntry" = :vParam1
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND "Detalle"."Project" LIKE '%' || :vParam2 || '%'
            ORDER BY "Project" ASC;
        END IF;


    ELSEIF vTipo = 'Get_DocDetRIN' THEN

        IF :vParam3 = '1' THEN
            SELECT
                IFNULL("Detalle"."ItemCode", "Detalle"."U_MGS_LC_SERCOM") AS "ItemCode",
                IFNULL("Detalle"."U_MGS_CL_NITEMPYP",'') AS "U_MGS_CL_NITEMPYP",
                "Detalle"."Quantity",
                "Detalle"."Price",
                IFNULL("Detalle"."U_MGS_CL_CANINI", 0) AS "cantidad_Inicial",
                IFNULL("Detalle"."U_MGS_CL_PREINI", 0) AS "costo_Unit_Inicial",
                "Detalle"."Project",
                IFNULL("Detalle"."U_MGS_CL_TIPBENPRO",'') AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."Quantity" * "Detalle"."Price") AS "LineTotal",
                "Detalle"."Quantity" * 100 AS "Porcentaje",
                IFNULL(t2."Name",'') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE",'') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name",'') AS "EstadoProyecto"
            FROM "RIN1" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."Project" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."DocEntry" = :vParam1
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND "Detalle"."Project" LIKE '%' || :vParam2 || '%'
            UNION ALL
            SELECT
                "Detalle"."U_MGS_CL_TARIID" AS "ItemCode",
                '' AS "U_MGS_CL_NITEMPYP",
                "Detalle"."U_MGS_CL_CANTID" AS "Quantity",
                "Detalle"."U_MGS_CL_COSUNI" AS "Price",
                0 AS "cantidad_Inicial",
                0 AS "costo_Unit_Inicial",
                IFNULL("Detalle"."U_MGS_CL_CODPRY", '') AS "Project",
                '' AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."U_MGS_CL_COSTOT") AS "LineTotal",
                0 AS "Porcentaje",
                IFNULL(t2."Name", '') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE", '') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name", '') AS "EstadoProyecto"
            FROM "@MGS_CL_HISVEN" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."U_MGS_CL_CODPRO" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."U_MGS_CL_NRODOC" = :vParam4
              AND "Detalle"."U_MGS_CL_CODPRY" LIKE '%' || :vParam2 || '%'
            ORDER BY "Project" ASC;
        ELSE
            SELECT
                IFNULL("Detalle"."ItemCode", "Detalle"."U_MGS_LC_SERCOM") AS "ItemCode",
                IFNULL("Detalle"."U_MGS_CL_NITEMPYP",'') AS "U_MGS_CL_NITEMPYP",
                "Detalle"."Quantity",
                "Detalle"."Price",
                IFNULL("Detalle"."U_MGS_CL_CANINI", 0) AS "cantidad_Inicial",
                IFNULL("Detalle"."U_MGS_CL_PREINI", 0) AS "costo_Unit_Inicial",
                "Detalle"."Project",
                IFNULL("Detalle"."U_MGS_CL_TIPBENPRO",'') AS "U_MGS_CL_TIPBENPRO",
                ("Detalle"."Quantity" * "Detalle"."Price") AS "LineTotal",
                "Detalle"."Quantity" * 100 AS "Porcentaje",
                IFNULL(t2."Name",'') AS "UnidadNegocio",
                IFNULL(t1."U_MGS_CL_JEFE",'') AS "JefeCuenta",
                IFNULL(t4."Name",'') AS "Familia",
                IFNULL(t3."Name",'') AS "EstadoProyecto"
            FROM "RIN1" "Detalle"
            LEFT JOIN "OPRJ" t1 ON "Detalle"."Project" = t1."PrjCode"
            LEFT JOIN "@MGS_CL_UNINEG" t2 ON t1."U_MGS_CL_UNINEG" = t2."Code"
            LEFT JOIN "@MGS_CL_ESTPRO" t3 ON t3."Code" = t1."U_MGS_CL_ESTPRO"
            LEFT JOIN "@MGS_CL_FAMILI" t4 ON t4."Code" = t1."U_MGS_CL_FAMILI"
            WHERE "Detalle"."DocEntry" = :vParam1
              AND IFNULL("Detalle"."U_MGS_CL_ESTADO",'') <> '01'
              AND "Detalle"."Project" LIKE '%' || :vParam2 || '%'
            ORDER BY "Project" ASC;
        END IF;


    ELSEIF vTipo = 'Get_AmtAvail' THEN

        SELECT
            TABLA."contrato",
            TABLA."Project",
            TABLA."categoriaCosto",
            TABLA."LineNum",
            TABLA."montoPresu",
            TABLA."montoOC",
            TABLA."montoFact",
            (TABLA."montoPresu" - TABLA."montoOC" - TABLA."montoFact") AS "montoDisp"
        FROM (
            SELECT
                IFNULL(T0."U_MGS_CL_CODINTPYP",'') AS "contrato",
                T1."Project",
                T1."ItemCode" AS "categoriaCosto",
                T1."LineNum",
                SUM(T1."LineTotal") AS "montoPresu",
                IFNULL((
                    SELECT SUM(T2."LineTotal")
                    FROM "OPOR" T3
                    INNER JOIN "POR1" T2 ON T3."DocEntry" = T2."DocEntry"
                    WHERE T3."CANCELED" = 'N'
                      AND T2."Project" = T1."Project"
                      AND T2."ItemCode" = T1."ItemCode"
                ), 0) AS "montoOC",
                IFNULL((
                    SELECT SUM("LineTotal")
                    FROM "OPCH" T4
                    INNER JOIN "PCH1" T5 ON T4."DocEntry" = T5."DocEntry"
                    WHERE T5."BaseType" = -1
                      AND T4."CANCELED" = 'N'
                      AND T5."Project" = T1."Project"
                      AND T5."ItemCode" = T1."ItemCode"
                ), 0) AS "montoFact"
            FROM "OPRQ" T0
            INNER JOIN "PRQ1" T1 ON T0."DocEntry" = T1."DocEntry"
            WHERE T0."CANCELED" = 'N'
              AND (T0."DocStatus" = 'O' OR (T0."DocStatus" = 'C' AND IFNULL(T0."U_MGS_CL_CODINTPYP",'') <> ''))
              AND IFNULL(T1."U_MGS_CL_ESTADO",'') <> '01'
              AND T1."Project" = :vParam1
            GROUP BY T0."U_MGS_CL_CODINTPYP", T1."Project", T1."ItemCode", T1."LineNum"
        ) AS TABLA
        ORDER BY TABLA."LineNum";


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
