module Main
open Tester
open System.Net.Http

[<EntryPoint>]
let main argv =
    let testsuite = Route("http://localhost:8080",[
        Method(HttpMethod.Get, [
            Route("math", [
                Route("sum", [
                    Test([("a","1"); ("b", "2")], "3");
                    Test([("a","-4"); ("b", "5")], "1")
                ])
            ]);
            Route("non_existing", [
                Test([], "1")
            ]);
            Route("abc", [
                Route("upper", [
                    Test([("word", "juha")], "JUHA")
                ])
            ])
        ])
    ])

    let builder = {
        Method = None
        Uri =  None
        Expected =  None
        Parameters =  None
        Client = new HttpClient()
    }

    buildTests builder testsuite
    |> Async.Parallel
    |> Async.RunSynchronously
    |> printResults

    
    0 // return an integer exit code
