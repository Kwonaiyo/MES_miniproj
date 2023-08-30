```SQL
USE [KDTB03_MES_2]
GO
/****** Object:  StoredProcedure [dbo].[TGR_QM_ErrorInquire_S1]    Script Date: 2023-08-30 오전 11:37:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		권문규
-- Create date: 2023-06-28
-- Description:	공장별 최종 불량률 조회
-- =============================================
ALTER PROCEDURE [dbo].[TGR_QM_ErrorInquire_S1]
	@PLANTCODE      VARCHAR(10),  -- 공장     
	@STARTDATE      DATETIME,  -- 시작일자
	@ENDDATE        VARCHAR(10),  -- 종료일자      

	@LANG    VARCHAR(10),
	@RS_CODE VARCHAR(1)   OUTPUT,
	@RS_MSG  VARCHAR(200) OUTPUT
	
AS
BEGIN
	SELECT A.PLANTCODE	                                                          AS PLANTCODE
	      ,A.TOTBADQTY                                                            AS TOTBADQTY
	      ,B.QTYOFROH                                                             AS QTYOFROH
		  ,C.QTYOFPROCESS                                                         AS QTYOFPROCESS
		  ,CONVERT(VARCHAR, ROUND((B.QTYOFROH / A.TOTBADQTY), 4) * 100) + '%'     AS ROHRATE
		  ,CONVERT(VARCHAR, ROUND((C.QTYOFPROCESS / A.TOTBADQTY), 4) * 100) + '%' AS PROCESSRATE
	  FROM (SELECT PLANTCODE              AS PLANTCODE
        	      ,SUM(ISNULL(DELQTY, 0)) AS TOTBADQTY
        	  FROM TB_Deleted WITH(NOLOCK)
        	 WHERE PLANTCODE LIKE '%' + @PLANTCODE
        	   AND DELDATE BETWEEN @STARTDATE AND @ENDDATE
          GROUP BY PLANTCODE) A
	  JOIN ( SELECT PLANTCODE              AS PLANTCODE	
	               ,SUM(ISNULL(DELQTY, 0)) AS QTYOFROH
	           FROM TB_Deleted WITH(NOLOCK)
	          WHERE PLANTCODE LIKE '%' + @PLANTCODE
	            AND DELDATE BETWEEN @STARTDATE AND @ENDDATE
	            AND DELWHY = 'FL_ROH'
	       GROUP BY PLANTCODE) B
		ON A.PLANTCODE = B.PLANTCODE
	  JOIN ( SELECT PLANTCODE              AS PLANTCODE	
	               ,SUM(ISNULL(DELQTY, 0)) AS QTYOFPROCESS
	           FROM TB_Deleted WITH(NOLOCK)
	          WHERE PLANTCODE LIKE '%' + @PLANTCODE
	            AND DELDATE BETWEEN @STARTDATE AND @ENDDATE
	            AND DELWHY = 'FL_PROCESS'
	       GROUP BY PLANTCODE ) C
		ON A.PLANTCODE = C.PLANTCODE

END

```