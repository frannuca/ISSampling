namespace ISSampling

open System
open Accord.Statistics.Distributions
open MathNet.Numerics.LinearAlgebra

module Functions=
    open Accord.Math.Optimization
    open Accord.Math.Convergence
    open CenterSpace.NMath.Analysis
    open CenterSpace.NMath.Core

    let private standardnormal = new Univariate.NormalDistribution()
    let Phi = standardnormal.DistributionFunction
    let InvPhi = standardnormal.InverseDistributionFunction

    let Pk_z(Ak:Vector<float>,pk:double)(z:Vector<float>)=
        let ak2 = (Ak.ToRowMatrix()*Ak.ToColumnMatrix()).[0,0]
        let bk = Math.Sqrt(1.0 - ak2)

        let num = (Ak.ToRowMatrix()*z.ToColumnMatrix()+InvPhi(pk)).[0,0]
        Phi(num/bk)

    let FuncIS(q:double,A:Matrix<float>,Pk:Vector<float>,ck:Vector<float>)(zarr:float array)=        
        let ck2 = ck.PointwiseMultiply(ck)
        let z = Vector<float>.Build.DenseOfArray zarr
        let pzarr = [0 .. A.RowCount-1] |> Seq.map(fun i -> Pk_z(A.Row(i),Pk.[i])(z) ) |> Array.ofSeq
        let pz = Vector<float>.Build.DenseOfArray pzarr

        let pp = pz.PointwiseMultiply(1.0-pz)

        let E = (pz.ToRowMatrix()*ck.ToColumnMatrix()).[0,0]
        let V2 = (pp.ToRowMatrix()*ck2.ToColumnMatrix()).[0,0]

        let V = Math.Sqrt(V2)
        let s = (q-E)/V
        //printfn "%A" s
        
        let ppp = Phi(s)
        let z2 = z.ToRowMatrix()*z.ToColumnMatrix()
        let a = (1.0-Phi(s))*Math.Exp(-z2.[0,0]*0.5)
        //printfn "%A -> %A" (String.Join(",",z.ToArray()))  a
        //printfn "%A" a
        a

         
    let FindApproxSolution(q:double,A:Matrix<float>,Pk:Vector<float>,ck:Vector<float>)=
        let algorithm = NLoptNet.NLoptAlgorithm.LN_BOBYQA
        let solver = new NLoptNet.NLoptSolver(algorithm,(uint32)A.ColumnCount,1e-12,1000000)
        solver.SetLowerBounds([0 .. A.ColumnCount-1] |> Seq.map(fun _ -> 0.0) |> Array.ofSeq)
        solver.SetUpperBounds([0 .. A.ColumnCount-1] |> Seq.map(fun _ -> 5.0) |> Array.ofSeq)
        let csharpfunc:Func<float[],float> = System.Func<_,_>(fun x -> FuncIS(q,A,Pk,ck)(x)- 0.5)
        let data = [|0 .. A.ColumnCount-1|] |> Array.map(fun _ ->  5.0)
        solver.SetMaxObjective(csharpfunc)

    let Optimize(q:double,A:Matrix<float>,Pk:Vector<float>,ck:Vector<float>)=
        let func = System.Func<_,_>(fun (x:DoubleVector) -> FuncIS(q,A,Pk,ck)(x.ToArray()))

        let algorithm = NLoptNet.NLoptAlgorithm.LN_AUGLAG
        let solver = new NLoptNet.NLoptSolver(algorithm,(uint32)A.ColumnCount,1e-12,1000000)
        solver.SetLowerBounds([0 .. A.ColumnCount-1] |> Seq.map(fun _ -> 0.0) |> Array.ofSeq)
        //solver.SetUpperBounds([0 .. A.ColumnCount-1] |> Seq.map(fun _ -> 5.0) |> Array.ofSeq)
        let csharpfunc:Func<float[],float> = System.Func<_,_>(fun x -> FuncIS(q,A,Pk,ck)(x))
        let data = [|0 .. A.ColumnCount-1|] |> Array.map(fun _ ->  10.0)
        solver.SetMaxObjective(csharpfunc)

        let res, xout = solver.Optimize(data)
        data

