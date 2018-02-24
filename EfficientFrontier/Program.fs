// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open ISSampling.EfficientFrontier
open Functions
open MathNet.Numerics.LinearAlgebra
open System.Data
open Deedle

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    let rarr = [|
                    [|1.00;0.62;0.31;0.47;0.79;0.85;0.80|];
                    [|0.62;1.00;1.00;0.47;0.79;0.85;0.80|];
                    [|0.31;1.00;1.00;0.47;0.79;0.85;0.80|];
                    [|0.47;0.47;0.47;1.0;0.79;0.85;0.80|];
                    [|0.79;0.79;0.79;0.79;1.0;0.85;0.80|];
                    [|0.85;0.85;0.85;0.85;0.85;1.0;0.80|];
                    [|0.80;0.80;0.80;0.80;0.80;0.80;1.0|];
                    |]
    let R = Matrix<float>.Build.DenseOfRowArrays(rarr)
    let erc0 = [|41871.0;10.0;12010.0;1240.0;1681153358.0;243761.0;53228.0|]
    let erc = Vector<float>.Build.DenseOfArray erc0
    let roe = [|0.09;0.18;0.12;0.02;0.15;0.20;0.0|]
    let RoE = Vector<float>.Build.DenseOfArray roe
    let frame = MonteCarlo([|"GM";"SUB";"IWM";"SRU";"APAC";"IBCM";"SS"|],erc,10000,0.8,R,RoE)
    frame.SaveCsv(@"E:/mcefficient.csv")
    //Writing montecarlo
    0 // return an integer exit code
