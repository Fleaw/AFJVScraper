namespace AFJVScrapper

open System
open OfficeOpenXml
open System.IO
open Models
open Helpers

module ExcelCreator =

    [<Literal>]
    let fileName = "Annuaire.xlsx"

    [<Literal>]
    let sheetName = "Annuaire"

    ExcelPackage.LicenseContext <- LicenseContext.NonCommercial

    let header =
        [ [|"Nom"; "Département"; "Ville"; "Addresse"; "Site web"; "Contact"|] |> Array.map upcastToObj ]

    let createExcelFile fileName sheetName =
        let p = new ExcelPackage()
        let ws = p.Workbook.Worksheets.Add sheetName

        let headerRange = ws.Cells.["A1:F1"]

        header |> headerRange.LoadFromArrays |> ignore

        headerRange.Style.Font.Size <- 14 |> float32
        headerRange.Style.Font.Bold <- true
        headerRange.Style.HorizontalAlignment <- Style.ExcelHorizontalAlignment.Center
        headerRange.Style.VerticalAlignment <- Style.ExcelVerticalAlignment.Center

        FileInfo(fileName) |> p.SaveAs
        p.Dispose()

    let studioToList (studio:Studio) =
        let addressFormat = sprintf "%s\n%s %s" studio.Address.StreetAddress studio.Address.PostalCode studio.Address.City

        let contact = sprintf "%s\n%s" studio.Phone studio.Email

        [[|
            studio.Name
            studio.Department
            studio.Address.City
            addressFormat
            studio.Website
            contact
        |]
        |> Array.map upcastToObj
        ]

    let populateExcelWithStudio (excel:ExcelPackage) (sheetName:string) rowIndex studio =
        let ws = excel.Workbook.Worksheets.[sheetName]
        
        let rowNumber = rowIndex + 2

        studio
        |> studioToList
        |> ws.Cells.[rowNumber, 1(*A*),rowNumber, 7(*F*)].LoadFromArrays
        |> ignore

        if studio.AFJVUrl <> String.Empty then
            ws.Cells.[rowNumber, 1(*A*)].Hyperlink <- Uri (formatUrl studio.AFJVUrl)

        if studio.GoogleMapUrl <> String.Empty then
            ws.Cells.[rowNumber, 4(*D*)].Hyperlink <- Uri (formatUrl studio.GoogleMapUrl)

        if studio.Website <> String.Empty then
            ws.Cells.[rowNumber, 5(*E*)].Hyperlink <- Uri (formatUrl studio.Website)

        ()

    let importStudiosToExcel (studios:Studio list) =
        createExcelFile fileName sheetName
        let excelFile = new ExcelPackage(FileInfo(fileName))

        studios
        |> List.mapi (populateExcelWithStudio excelFile sheetName) 
        |> ignore

        excelFile.Save()
        excelFile.Dispose()