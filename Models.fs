namespace AFJVScrapper

open System

module Models =

    type Address =
        {
            City : string
            StreetAddress : string
            PostalCode : string
            Country : string
        }

    type Studio =
        {
            Name : string
            AFJVUrl : string
            Address : Address
            GoogleMapUrl : string
            Department : string
            Phone : string
            Email : string
            Website : string
        }