```SQL
-- =============================================
-- Author:		권문규
-- Create date: 2023-06-26
-- Description:	생산실적 등록 시에 임시불량품들에 대한 원자재LOT를 얻기 위한 프로시져
-- =============================================
ALTER PROCEDURE [dbo].[TGR_GET_CLOT]
	@FAULTNO VARCHAR(50), -- 임시불량품의 불량판정번호(FAULTNO)

	@LANG	 VARCHAR(10) = 'KO',
	@RS_CODE VARCHAR(1)	  OUTPUT,
	@RS_MSG	 VARCHAR(200) OUTPUT

AS
BEGIN
	DECLARE @CLOTNO VARCHAR(50)
	SELECT @CLOTNO = ISNULL(CLOTNO, 'X')
	  FROM TB_Fault WITH(NOLOCK)
	 WHERE FAULTNO = @FAULTNO

	SET @RS_MSG = @CLOTNO
END
```