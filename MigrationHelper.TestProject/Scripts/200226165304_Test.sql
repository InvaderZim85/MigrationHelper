SELECT
	*
FROM
	myTable;
	
DECLARE
	@variable INT;
	
SET @variable = 10;

SELECT
	@@Version AS Version,
	@variable AS Test;