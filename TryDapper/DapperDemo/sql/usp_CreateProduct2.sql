USE [Northwind]
GO

/****** Object:  StoredProcedure [dbo].[usp_CreateProduct2]    Script Date: 26-May-15 00:01:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_CreateProduct2] @ProductName nvarchar(40),
											@SupplierID int,   
											@CategoryID int,
											@QuantityPerUnit nvarchar(20),
											@UnitPrice money,
											@UnitsInStock smallint,
											@UnitsOnOrder smallint,
											@ReorderLevel smallint,
											@Discontinued bit
AS
BEGIN
	INSERT INTO Products (ProductName, SupplierID, CategoryID, QuantityPerUnit, UnitPrice, UnitsInStock, UnitsOnOrder, ReorderLevel, Discontinued)
	VALUES (@ProductName, @SupplierID, @CategoryID, @QuantityPerUnit, @UnitPrice, @UnitsInStock, @UnitsOnOrder, @ReorderLevel, @Discontinued)

	SELECT CAST(SCOPE_IDENTITY() AS INT)
END
GO


