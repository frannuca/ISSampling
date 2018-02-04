// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open ISSampling
open MathNet.Numerics.LinearAlgebra
open Deedle
open System.IO

[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    let m = 1000
    let d = 10

    let Pk = [|0 .. m-1|] |> Array.map(fun k -> 0.01*(1.0+Math.Sin(16.0*Math.PI*float(k)/float(m)))) |> Vector<float>.Build.DenseOfArray
    let Ck = [|0 .. m-1|] |> Array.map(fun k -> Math.Ceiling(5.0*float(k)/float(m))**2.0)  |> Vector<float>.Build.DenseOfArray


    let nsims =  5000
    let nsims2 = 100000
    let L = 1000.0
    let randomU = new Accord.Statistics.Distributions.Univariate.UniformContinuousDistribution(new Accord.DoubleRange(0.0,1.0/Math.Sqrt(float(d))))
    let Ak =  Array2D.init m d (fun i j -> randomU.Generate()) |> Matrix<float>.Build.DenseOfArray

    //calculated factor shift:
    let mu = Functions.Optimize(L,Ak,Pk,Ck) |> Vector<float>.Build.DenseOfArray
    let Lmax = 5000

    let loss=1200.0
    let P_is_lr_L,Loss_IS = MonteCarlo.Run(loss,Ak,mu,nsims,Pk.ToArray(),Ck.ToArray())
    let P_lr_L,Loss_Simple = MonteCarlo.RunSimple(loss,Ak,nsims2,Pk.ToArray(),Ck.ToArray())  
    printfn "IS P(L>%A)=%A" loss P_is_lr_L
    printfn "NR P(L>%A)=%A" loss P_lr_L                                            
    let framecIS = Loss_IS|> Seq.map(fun  (i,lss,x) -> i,lss,x) |> Frame.ofRecords
    let framec = Loss_Simple  |> Seq.mapi(fun i (x,y) -> i,x,y) |> Frame.ofRecords
    framec.SaveCsv("E:/SmpleChart_B.csv")
    framecIS.SaveCsv("E:/ISChart_B.csv")

    let q = MonteCarlo.computeQuantileOnIS(Loss_IS |> Seq.map(fun (i,a,b) -> a,b)|> Array.ofSeq,1e-5)(0.9997)

    //let curve = [0 .. 10] |> Seq.map(fun i ->
    //                                            let loss = L+(float(Lmax) - L)*float(i)/200.0
    //                                            let P_is_lr_L = MonteCarlo.Run(loss,Ak,mu,nsims,Pk.ToArray(),Ck.ToArray())
    //                                            let P_lr_L,_ = MonteCarlo.RunSimple(loss,Ak,nsims2,Pk.ToArray(),Ck.ToArray())  
    //                                            printfn "IS P(L>%A)=%A" loss P_is_lr_L
    //                                            printfn "NR P(L>%A)=%A" loss P_lr_L

    //                                            (i,loss,P_lr_L,P_is_lr_L)
    //                                        )
    //                       |> Array.ofSeq
    
    //let frame = curve |> Frame.ofRecords
    //frame.SaveCsv("E:/MCurves_1000000.csv")
    //running montecarlos:
    //let mutable auxnsim = nsims
    //let mutable MCres = 10.0
    //for ns in 0 .. 5  do  
    //    if MCres>0.03 then
    //        let P_lr_L,quantiles = MonteCarlo.RunSimple(L,Ak,nsims2,Pk.ToArray(),Ck.ToArray())        
    //        let namefile = sprintf "quantile_%A.csv" auxnsim
    //        use writer=new StreamWriter("E:/"+namefile)
    //        quantiles |> Seq.iter(fun (x,y) -> writer.WriteLine (sprintf "%A,%A" x y))
    //        auxnsim <- auxnsim*2
    //        MCres <- Math.Abs((P_is_lr_L-P_lr_L)/P_is_lr_L)
    //        printfn "IS P(L>%A)=%A" L P_is_lr_L
    //        printfn "NR P(L>%A)=%A" L P_lr_L

            
        

    
    0 // return an integer exit code
