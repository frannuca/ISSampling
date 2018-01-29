namespace ISSampling

open System
open MathNet.Numerics.LinearAlgebra
open Accord.Statistics.Distributions

module MonteCarlo=
    let Run(x:float,A:Matrix<float>,mu:Vector<float>,nsim:int,pk:float array,ck:float array)=
        
        let distributions = mu |> Seq.map(fun u ->  new Univariate.NormalDistribution(u))
        let e = new Univariate.NormalDistribution()
        let barriers = pk |> Array.map(fun p -> e.InverseDistributionFunction(1.0-p)) 
        let getZ()=
            distributions |> Seq.map(fun d -> d.Generate())
        let mu2 = (mu.ToRowMatrix()*mu.ToColumnMatrix()*0.5)
        let mutable acc = 0.0
        for n in 0 .. nsim-1 do
            let Z = Vector.Build.DenseOfArray (getZ()|> Array.ofSeq)
            let X = [0 .. A.RowCount-1] |> Seq.map(fun i->
                                                            let systematic = A.Row(i).ToRowMatrix()*Z.ToColumnMatrix()
                                                            let idiosyncratic = e.Generate() * Math.Sqrt(1.0 - (A.Row(i).PointwiseMultiply(A.Row(i))).Sum())
                                                            systematic.[0,0] + idiosyncratic
                                                            )
                                         |> Array.ofSeq
            


            let L = X 
                    |> Array.mapi(fun i x -> if x > barriers.[i] then ck.[i] else 0.0)
                    |> Array.sum
            
            if L > x then
                let lr = -mu.ToRowMatrix()*Z.ToColumnMatrix()+ mu2
                let elr = Math.Exp(lr.[0,0])
                acc <- acc + elr
            
        
        acc/float(nsim)

    
    let RunSimple(x:float,A:Matrix<float>,nsim:int,pk:float array,ck:float array)=       
        
        let e = new Univariate.NormalDistribution()
        let barriers = pk |> Array.map(fun p -> e.InverseDistributionFunction(1.0-p)) 
        let getZ()=
            [0 .. A.ColumnCount-1] |> Seq.map(fun _ -> e.Generate()) |> Array.ofSeq
        
        let mutable acc = 0.0
        let LossD = new System.Collections.Generic.List<double>()

        for n in 0 .. nsim-1 do
            let Z = Vector.Build.DenseOfArray (getZ()|> Array.ofSeq)
            let X = [0 .. A.RowCount-1] |> Seq.map(fun i->
                                                            let systematic = A.Row(i).ToRowMatrix()*Z.ToColumnMatrix()
                                                            let idiosyncratic = e.Generate() * Math.Sqrt(1.0 - (A.Row(i).PointwiseMultiply(A.Row(i))).Sum())
                                                            systematic.[0,0] + idiosyncratic
                                                            )
                                         |> Array.ofSeq
            


            let L = X 
                    |> Array.mapi(fun i x -> if x > barriers.[i] then ck.[i] else 0.0)
                    |> Array.sum
            
            if L > x then
                acc <- acc + 1.0
            
            LossD.Add(L)
        
        acc/float(nsim), LossD |> Seq.sort |> Seq.mapi(fun i l -> l, float(i)/float(nsim))

