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
        WHERE "Active" = 'Y' and "PrjCode" <> 'GENERICO'
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
        WHERE P."Active" = 'Y' and P."PrjCode" <> 'GENERICO'
        ORDER BY P."PrjCode", P."PrjName";

    ELSEIF vTipo = 'Get_GesTiendas' THEN

        SELECT
            "PrjCode" AS "Codigo",
            "PrjName" AS "Nombre"
        FROM "OPRJ"
        WHERE "Active" = 'Y' and "PrjCode" <> 'GENERICO'
        ORDER BY "PrjCode";


    ELSEIF vTipo = 'Get_GesTipos' THEN

        SELECT
            "Code" AS "Code",
            "Name" AS "Name"
        FROM "@MGS_CL_GESTIPO"
        WHERE IFNULL("U_MGS_CL_ACTIVO", 'NO') = 'SI'
        ORDER BY "Code";


    ELSEIF vTipo = 'Get_GesUltPer' THEN

        SELECT
            CASE
                WHEN MAX(TO_DATE('01-' || "U_MGS_CL_PERIODO", 'DD-MM-YYYY')) IS NULL THEN ''
                ELSE TO_VARCHAR(MAX(TO_DATE('01-' || "U_MGS_CL_PERIODO", 'DD-MM-YYYY')), 'MM-YYYY')
            END AS "U_MGS_CL_PERIODO"
        FROM "@MGS_CL_GESCAB";


    ELSEIF vTipo = 'Get_GesCab' THEN

        SELECT
            "DocEntry" AS "DocEntry",
            "U_MGS_CL_PERIODO" AS "U_MGS_CL_PERIODO"
        FROM "@MGS_CL_GESCAB"
        WHERE TO_VARCHAR("U_MGS_CL_PERIODO", 'YYYY-MM') = :vParam1;


    ELSEIF vTipo = 'Get_GesDet' THEN

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
            D."U_MGS_CL_CVENTA" AS "U_MGS_CL_CVENTA",
            D."U_MGS_CL_CRENTA" AS "U_MGS_CL_CRENTA",
            D."U_MGS_CL_CVAN" AS "U_MGS_CL_CVAN",
            D."U_MGS_CL_CPERSO" AS "U_MGS_CL_CPERSO",
            D."U_MGS_CL_CGESTI" AS "U_MGS_CL_CGESTI",
            D."U_MGS_CL_CSERV" AS "U_MGS_CL_CSERV",
            D."U_MGS_CL_CCC" AS "U_MGS_CL_CCC",
            D."U_MGS_CL_CADM" AS "U_MGS_CL_CADM"
        FROM "@MGS_CL_GESCAB" C
        JOIN "@MGS_CL_GESDET" D
          ON D."DocEntry" = C."DocEntry"
        WHERE TO_VARCHAR(C."U_MGS_CL_PERIODO", ''YYYY-MM'') = ''' || :vParam1 || '''
          AND D."U_MGS_CL_TIENDA" IN (' || lvTiendas || ')
        ORDER BY
            D."LineId", D."U_MGS_CL_TIENDA", D."U_MGS_CL_NOMTIE"
    ';
    
      EXECUTE IMMEDIATE :lvSql;


      

    ELSEIF vTipo = 'Get_GesPreview' THEN

        DECLARE lvPeriodoBaseDate DATE;
        DECLARE lvPeriodoBase NVARCHAR(10);
        DECLARE lvPeriodoDestino NVARCHAR(10);
        DECLARE lvTipoDefault NVARCHAR(20);

								 
								 
								
  
        SELECT MAX("U_MGS_CL_PERIODO")
          INTO lvPeriodoBaseDate
          FROM "@MGS_CL_GESCAB";

        IF lvPeriodoBaseDate IS NULL THEN
            lvPeriodoBase := TO_VARCHAR(CURRENT_DATE, 'MM-YYYY');
            lvPeriodoDestino := TO_VARCHAR(CURRENT_DATE, 'MM-YYYY');
        ELSE
            lvPeriodoBase := TO_VARCHAR(lvPeriodoBaseDate, 'MM-YYYY');
            lvPeriodoDestino := TO_VARCHAR(ADD_MONTHS(lvPeriodoBaseDate, 1), 'MM-YYYY');
        END IF;

        SELECT "Code"
          INTO lvTipoDefault
          FROM "@MGS_CL_GESTIPO"
         WHERE IFNULL("U_MGS_CL_ACTIVO", 'NO') = 'SI'
           AND UPPER("Name") = 'POR DEFECTO'
         ORDER BY "Code"
         LIMIT 1;

        SELECT
            :lvPeriodoBase AS "U_MGS_CL_PERIODO",
            :lvPeriodoDestino AS "U_MGS_CL_PERIODO_DEST",
            P."PrjCode" AS "U_MGS_CL_TIENDA",
            P."PrjName" AS "U_MGS_CL_NOMTIE",
            IFNULL(G."DocEntry", 0) AS "DocEntry",
            IFNULL(G."LineId", 0) AS "LineId",
            IFNULL(G."U_MGS_CL_CVENTA", :lvTipoDefault) AS "U_MGS_CL_CVENTA",
            IFNULL(G."U_MGS_CL_CRENTA", :lvTipoDefault) AS "U_MGS_CL_CRENTA",
            IFNULL(G."U_MGS_CL_CVAN", :lvTipoDefault) AS "U_MGS_CL_CVAN",
            IFNULL(G."U_MGS_CL_CPERSO", :lvTipoDefault) AS "U_MGS_CL_CPERSO",
            IFNULL(G."U_MGS_CL_CGESTI", :lvTipoDefault) AS "U_MGS_CL_CGESTI",
            IFNULL(G."U_MGS_CL_CSERV", :lvTipoDefault) AS "U_MGS_CL_CSERV",
            IFNULL(G."U_MGS_CL_CCC", :lvTipoDefault) AS "U_MGS_CL_CCC",
            IFNULL(G."U_MGS_CL_CADM", :lvTipoDefault) AS "U_MGS_CL_CADM"
        FROM "OPRJ" P
        LEFT JOIN (
            SELECT
                C."DocEntry",
                D."LineId",
                D."U_MGS_CL_TIENDA",
                D."U_MGS_CL_NOMTIE",
                D."U_MGS_CL_CVENTA",
                D."U_MGS_CL_CRENTA",
                D."U_MGS_CL_CVAN",
                D."U_MGS_CL_CPERSO",
                D."U_MGS_CL_CGESTI",
                D."U_MGS_CL_CSERV",
                D."U_MGS_CL_CCC",
                D."U_MGS_CL_CADM"
            FROM "@MGS_CL_GESCAB" C
            JOIN "@MGS_CL_GESDET" D
              ON D."DocEntry" = C."DocEntry"
            WHERE TO_VARCHAR(C."U_MGS_CL_PERIODO", 'YYYY-MM') = :lvPeriodoBase
        ) G ON G."U_MGS_CL_TIENDA" = P."PrjCode"
        WHERE P."Active" = 'Y' and P."PrjCode" <> 'GENERICO'
        ORDER BY P."PrjCode", P."PrjName";
 


    ELSEIF vTipo = 'Get_VanTienda' THEN

        SELECT
            "PrjCode" AS "PrjCode",
            "PrjName" AS "PrjName"
        FROM "OPRJ"
        WHERE "Active" = 'Y' and "PrjCode" <> 'GENERICO'
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
            IFNULL(D."U_MGS_CL_PORC", 0) AS "U_MGS_CL_PORC",
			IFNULL(D."U_MGS_CL_ACTIVO", 'NO') AS "U_MGS_CL_ACTIVO"													  
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
        WHERE  "PrjCode" <> 'GENERICO' and "PrjCode" = :vParam1;


											 
    ELSEIF vTipo = 'Get_VanCab' THEN

        SELECT
            "DocEntry" AS "DocEntry"
        FROM "@MGS_CL_VANTCAB"
        WHERE "U_MGS_CL_TIENDA" = :vParam1;

															   
    ELSEIF vTipo = 'Get_VanGrpDet' THEN

        SELECT
            D."LineId" AS "LineId",
            IFNULL(D."U_MGS_CL_ACTIVO",'NO') AS "U_MGS_CL_ACTIVO"
        FROM "@MGS_CL_VANTCAB" H
        JOIN "@MGS_CL_VANTDET" D ON D."DocEntry" = H."DocEntry"
        WHERE H."U_MGS_CL_TIENDA" = :vParam1
          AND D."U_MGS_CL_GRPCOD" = :vParam2
        ORDER BY D."LineId"
        LIMIT 1;


											
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

    ELSEIF vTipo = 'Get_PrmTienda' THEN

        SELECT
            "PrjCode" AS "PrjCode",
            "PrjName" AS "PrjName"
        FROM "OPRJ"
        WHERE "Active" = 'Y' and "PrjCode" <> 'GENERICO'
        ORDER BY "PrjCode";

    ELSEIF vTipo = 'Get_PrmGrupoM' THEN

        SELECT
            "Code" AS "Code",
            "Name" AS "Name"
        FROM "@MGS_CL_PRMGRP"
        WHERE IFNULL("U_MGS_CL_ACTIVO", 'NO') = 'SI'
        ORDER BY "Code";

    ELSEIF vTipo = 'Get_PrmTipGas' THEN

        SELECT
            "Code" AS "Code",
            "Name" AS "Name"
        FROM "TIENDAS_PASTIPIQUEOS"."@MGS_CL_TIPGAS"
        ORDER BY "Code";

    ELSEIF vTipo = 'Get_PrmTipMop' THEN

        SELECT
            "Code" AS "Code",
            "Name" AS "Name"
        FROM "TIENDAS_PASTIPIQUEOS"."@MGS_CL_TIPMOP"
        ORDER BY "Code";

    ELSEIF vTipo = 'Get_PrmItemM' THEN

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

    ELSEIF vTipo = 'Get_PrmTdaGrp' THEN

        SELECT
            D."DocEntry" AS "DocEntry",
            D."LineId" AS "LineId",
            D."U_MGS_CL_GRPCOD" AS "U_MGS_CL_GRPCOD",
            CASE
                WHEN IFNULL(D."U_MGS_CL_GRPNOM", '') = '' THEN G."Name"
                ELSE D."U_MGS_CL_GRPNOM"
            END AS "U_MGS_CL_GRPNOM",
            IFNULL(D."U_MGS_CL_TIPGAS", '') AS "U_MGS_CL_TIPGAS",
            IFNULL(D."U_MGS_CL_ACTIVO", 'NO') AS "U_MGS_CL_ACTIVO"
        FROM "@MGS_CL_PRMTCAB" H
        JOIN "@MGS_CL_PRMTDET" D ON D."DocEntry" = H."DocEntry"
        LEFT JOIN "@MGS_CL_PRMGRP" G ON G."Code" = D."U_MGS_CL_GRPCOD"
        WHERE H."U_MGS_CL_TIENDA" = :vParam1
          AND IFNULL(D."U_MGS_CL_ACTIVO",'NO') = 'SI'
        ORDER BY D."LineId";

    ELSEIF vTipo = 'Get_PrmGrpArt' THEN

        SELECT
            D."DocEntry" AS "DocEntry",
            D."LineId" AS "LineId",
            D."U_MGS_CL_GRPCOD" AS "U_MGS_CL_GRPCOD",
            D."U_MGS_CL_ITEMCOD" AS "U_MGS_CL_ITEMCOD",
            CASE
                WHEN IFNULL(D."U_MGS_CL_ITEMNAM", '') = '' THEN O."ItemName"
                ELSE D."U_MGS_CL_ITEMNAM"
            END AS "U_MGS_CL_ITEMNAM",
            IFNULL(D."U_MGS_CL_TIPGAS", '') AS "U_MGS_CL_TIPGAS",
            IFNULL(D."U_MGS_CL_TIPMOP", '') AS "U_MGS_CL_TIPMOP",
            IFNULL(D."U_MGS_CL_ACTIVO", 'NO') AS "U_MGS_CL_ACTIVO"
        FROM "@MGS_CL_PRMTIAD" D
        INNER JOIN "@MGS_CL_PRMTCAB" H ON D."DocEntry" = H."DocEntry"
        LEFT JOIN "OITM" O ON O."ItemCode" = D."U_MGS_CL_ITEMCOD"
        WHERE D."DocEntry" = :vParam1
          AND D."U_MGS_CL_GRPCOD" = :vParam2
          AND IFNULL(D."U_MGS_CL_ACTIVO",'NO') = 'SI'
        ORDER BY D."LineId";

    ELSEIF vTipo = 'Get_PrmCab' THEN

        SELECT
            "DocEntry" AS "DocEntry"
        FROM "@MGS_CL_PRMTCAB"
        WHERE "U_MGS_CL_TIENDA" = :vParam1;

    ELSEIF vTipo = 'Get_PrmGrpDet' THEN

        SELECT
            D."LineId" AS "LineId",
            IFNULL(D."U_MGS_CL_ACTIVO",'NO') AS "U_MGS_CL_ACTIVO"
        FROM "@MGS_CL_PRMTCAB" H
        JOIN "@MGS_CL_PRMTDET" D ON D."DocEntry" = H."DocEntry"
        WHERE H."U_MGS_CL_TIENDA" = :vParam1
          AND D."U_MGS_CL_GRPCOD" = :vParam2
        ORDER BY D."LineId"
        LIMIT 1;

    ELSEIF vTipo = 'Get_PrmGrpEx' THEN

        SELECT
            COUNT(1) AS "Total"
        FROM "@MGS_CL_PRMTCAB" H
        JOIN "@MGS_CL_PRMTDET" D ON D."DocEntry" = H."DocEntry"
        WHERE H."U_MGS_CL_TIENDA" = :vParam1
          AND D."U_MGS_CL_GRPCOD" = :vParam2
          AND IFNULL(D."U_MGS_CL_ACTIVO",'NO') = 'SI';

    ELSEIF vTipo = 'Get_PrmGrpNom' THEN

        SELECT
            "Name" AS "Name"
        FROM "@MGS_CL_PRMGRP"
        WHERE "Code" = :vParam1;

    ELSEIF vTipo = 'Get_Gas_Tiendas' THEN

        SELECT
            "PrjCode" AS "PrjCode",
            "PrjName" AS "PrjName"
        FROM "OPRJ"
        WHERE "Active" = 'Y' and "PrjCode" <> 'GENERICO'
        ORDER BY "PrjCode";

    ELSEIF vTipo = 'Get_Gas_ConceptosPRM' THEN

        SELECT
            "Code" AS "Code",
            "Name" AS "Name"
        FROM "@MGS_CL_PRMGRP"
        WHERE IFNULL("U_MGS_CL_ACTIVO", 'NO') = 'SI'
        ORDER BY "Code";

    ELSEIF vTipo = 'Get_Gas_TipMop' THEN

        SELECT
            "Code" AS "Code",
            "Name" AS "Name"
        FROM "TIENDAS_PASTIPIQUEOS"."@MGS_CL_TIPMOP"
        ORDER BY "Code";

    ELSEIF vTipo = 'Get_Gas_ItemM' THEN

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

    ELSEIF vTipo = 'Get_Gas_Buscar' THEN

		DECLARE lvGasTiendas NVARCHAR(5000);
        DECLARE lvGasSql     NVARCHAR(5000);

        lvGasTiendas := '''' || REPLACE(:vParam2, ',', ''',''') || '''';

        lvGasSql := '
        SELECT
            C."DocEntry" AS "DocEntry",
            D."LineId" AS "LineId",
            D."U_MGS_CL_TIENDA" AS "U_MGS_CL_TIENDA",
            D."U_MGS_CL_CONPRM" AS "U_MGS_CL_CONPRM",
            D."U_MGS_CL_TIPMOP" AS "U_MGS_CL_TIPMOP",
            D."U_MGS_CL_ITEMCOD" AS "U_MGS_CL_ITEMCOD",
            TO_VARCHAR(D."U_MGS_CL_FECHA", ''YYYY-MM-DD'') AS "U_MGS_CL_FECHA",
            D."U_MGS_CL_IMPORT" AS "U_MGS_CL_IMPORT",
            IFNULL(D."U_MGS_CL_NOVALI", ''NO'') AS "U_MGS_CL_NOVALI"
        FROM "@MGS_CL_GASCAB" C
        JOIN "@MGS_CL_GASDET" D ON D."DocEntry" = C."DocEntry"
        WHERE TO_VARCHAR(C."U_MGS_CL_PERIODO", ''YYYY-MM-DD'') = ''' || :vParam1 || '''';

        IF :vParam2 <> '' THEN
            lvGasSql := lvGasSql || ' AND D."U_MGS_CL_TIENDA" IN (' || lvGasTiendas || ')';
        END IF;

        IF :vParam3 <> '' AND :vParam3 <> 'TODOS' THEN
            lvGasSql := lvGasSql || ' AND D."U_MGS_CL_TIPMOP" = ''' || :vParam3 || '''';
        END IF;

        lvGasSql := lvGasSql || ' ORDER BY D."LineId"';

        EXECUTE IMMEDIATE :lvGasSql;

    ELSEIF vTipo = 'Get_Gas_UltimoDocEntryPeriodo' THEN

        SELECT
            "DocEntry" AS "DocEntry"
        FROM "@MGS_CL_GASCAB"
        WHERE TO_VARCHAR("U_MGS_CL_PERIODO", 'YYYY-MM') = :vParam1
        ORDER BY "DocEntry" DESC
        LIMIT 1;

    ELSEIF vTipo = 'Get_PrmItemTienda' THEN

        SELECT
            D."U_MGS_CL_GRPCOD" AS "U_MGS_CL_GRPCOD",
            CASE
                WHEN IFNULL(G."U_MGS_CL_GRPNOM", '') = '' THEN M."Name"
                ELSE G."U_MGS_CL_GRPNOM"
            END AS "U_MGS_CL_GRPNOM"
        FROM "@MGS_CL_PRMTIAD" D
        LEFT JOIN "@MGS_CL_PRMTDET" G
            ON G."DocEntry" = D."DocEntry"
           AND G."U_MGS_CL_GRPCOD" = D."U_MGS_CL_GRPCOD"
        LEFT JOIN "@MGS_CL_PRMGRP" M ON M."Code" = D."U_MGS_CL_GRPCOD"
        WHERE D."DocEntry" = :vParam1
          AND D."U_MGS_CL_ITEMCOD" = :vParam2
          AND IFNULL(D."U_MGS_CL_ACTIVO",'NO') = 'SI'
          AND (:vParam3 = '' OR D."U_MGS_CL_GRPCOD" <> :vParam3)
        LIMIT 1;

    ELSEIF vTipo = 'Get_PrmArtDet' THEN

        SELECT
            D."LineId" AS "LineId",
            IFNULL(D."U_MGS_CL_ACTIVO",'NO') AS "U_MGS_CL_ACTIVO"
        FROM "@MGS_CL_PRMTIAD" D
        WHERE D."DocEntry" = :vParam1
          AND D."U_MGS_CL_ITEMCOD" = :vParam2
          AND D."U_MGS_CL_GRPCOD" = :vParam3
        ORDER BY D."LineId"
        LIMIT 1;



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



    ELSEIF vTipo = 'Get_contactoEmpresa' THEN
    
        SELECT TOP 1
          (SELECT T1."U_MGS_CL_VALOR" FROM "@MGS_CL_CONDET" T1 WHERE  T1."Code" = T0."Code" and "U_MGS_CL_PARAM"  = 'telefono') AS Phone ,
          (SELECT T1."U_MGS_CL_VALOR" FROM "@MGS_CL_CONDET" T1  WHERE T1."Code" = T0."Code" and "U_MGS_CL_PARAM"  = 'correo')  AS Email,
          IFNULL(T0."U_MGS_CL_LOGO", '') AS LogoUrl
        FROM "@MGS_CL_CONFIG" T0;
                
    END IF;
    
    
END;
