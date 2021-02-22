module Main
open Tester
open System.Net.Http

[<EntryPoint>]
let main argv =
    let testsuite = Root ("http://localhost:8080",[
        Method(HttpMethod.Get, [
            Endpoint("math", [
                Endpoint("sum", [
                    Test([("a","1"); ("b", "2")], "3");
                    Test([("a","-4"); ("b", "5")], "1")
                ])
            ]);
            Endpoint("non_existing", [
                Test([], "1")
            ]);
            Endpoint("abc", [
                Endpoint("upper", [
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
