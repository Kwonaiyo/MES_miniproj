```SQL
USE [KDTB03_MES_2]
GO
/****** Object:  StoredProcedure [dbo].[TGR_QM_Fault_S1]    Script Date: 2023-08-30 오전 11:43:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		정원영
-- Create date: 2023-06-23
-- Description:	불량 재고 조회
-- =============================================
ALTER PROCEDURE [dbo].[TGR_QM_Fault_S1]
	 @PLANTCODE VARCHAR(10) -- 공장
	,@ITEMCODE	VARCHAR(30)	-- 품목
	,@STARTDATE VARCHAR(10) -- 시작일자
	,@ENDDATE   VARCHAR(10) -- 종료일자

	,@LANG		VARCHAR(10) = 'KO'
	,@RS_CODE	VARCHAR(1)   OUTPUT
	,@RS_MSG	VARCHAR(200) OUTPUT
AS
BEGIN
	SELECT 0								 AS CHK			   -- 불량확정 여부
		  ,A.PLANTCODE						 AS PLANTCODE	   -- 공장
		  ,A.WORKCENTERCODE					 AS WORKCENTERCODE -- 작업장
		  ,B.WORKCENTERNAME					 AS WORKCENTERNAME -- 작업장명
		  ,A.ITEMCODE						 AS ITEMCODE	   -- 품목
		  ,C.ITEMNAME						 AS ITEMNAME	   -- 품명
		  ,A.FAULTNO						 AS FAULTNO		   -- 불량판정번호
		  ,A.FAULTQTY						 AS FAULTQTY	   -- 불량수량
		  ,A.UNITCODE						 AS UNITCODE	   -- 단위
		  ,CONVERT(VARCHAR, A.MAKEDATE, 120) AS MAKEDATE	   -- 지시종료일시
		  ,DBO.FN_GET_USERNAME(A.MAKER)		 AS MAKER		   -- 등록자
	  FROM TB_Fault A WITH(NOLOCK) LEFT JOIN TB_WorkCenterMaster B
										  ON A.PLANTCODE	  = B.PLANTCODE
										 AND A.WORKCENTERCODE = B.WORKCENTERCODE
								   LEFT JOIN TB_ItemMaster C
										  ON A.PLANTCODE = C.PLANTCODE
										 AND A.ITEMCODE  = C.ITEMCODE
	 WHERE A.PLANTCODE LIKE '%' + @PLANTCODE
	   AND A.ITEMCODE  LIKE '%' + @ITEMCODE + '%'
	   AND A.INDATE BETWEEN @STARTDATE AND @ENDDATE
END
```