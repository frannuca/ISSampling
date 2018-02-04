namespace ISSampling

open System
open Accord.Statistics.Distributions
open MathNet.Numerics.LinearAlgebra

module Functions=
    open Accord.Math.Optimization
    open Accord.Math.Convergence
    //open NLoptNet
    open Accord.Math.Optimization
    open Accord.Math.Differentiation
    open System.Threading
    
    let private standardnormal = new Univariate.NormalDistribution()
    let Phi = standardnormal.DistributionFunction
    let InvPhi = standardnormal.InverseDistributionFunction

    let Pk_z(Ak:Vector<float>,pk:double)(z:Vector<float>)=
        let ak2 = (Ak.ToRowMatrix()*Ak.ToColumnMatrix()).[0,0]
        let bk = Math.Sqrt(1.0 - ak2)

        let num = (Ak.ToRowMatrix()*z.ToColumnMatrix()+InvPhi(pk)).[0,0]
        
        Phi(num/bk)

    let FuncIS_LOG(q:double,A:Matrix<float>,Pk:Vector<float>,ck:Vector<float>,token:CancellationTokenSource)(zarr:float array)=        
        if zarr |> Seq.exists(fun x -> x<>x) then
        //    token.Cancel()
            Double.NaN
        else
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
        
            let f1 = 1.0-Phi(s)
            let a = Math.Log(f1)+(-z2.[0,0]*0.5)
                        
            printfn "%A -> %A" (String.Join(",",z.ToArray()))  a
            a

               
    let Optimize_log(q:double,A:Matrix<float>,Pk:Vector<float>,ck:Vector<float>)=
        let token = new CancellationTokenSource()
        let fitness = System.Func<float[],float>(FuncIS_LOG(q,A,Pk,ck,token))
        let gfit = new FiniteDifferences(A.ColumnCount,fitness)
        let grad = System.Func<float[],float[]>(gfit.Gradient)
        let f = new NonlinearObjectiveFunction(A.ColumnCount,fitness,grad)
        let getConstraint(i:int): IConstraint[]=
                [|
                    new NonlinearConstraint(f,
                                            System.Func<float[],float>(fun x -> x.[i]),
                                            ConstraintType.GreaterThanOrEqualTo,
                                            1e-6,
                                            System.Func<float[],float[]>(fun x -> x|>Array.mapi(fun n v-> if n=i then 1.0 else 0.0)),
                                            1e-3) :> IConstraint;
                     new NonlinearConstraint(f,
                                            System.Func<float[],float>(fun x -> x.[i]),
                                            ConstraintType.LesserThanOrEqualTo,
                                            10.0,
                                            System.Func<float[],float[]>(fun x -> x|>Array.mapi(fun n v-> if n=i then 1.0 else 0.0)),
                                            1e-3) :> IConstraint
                   |]


        let data = [|0 .. A.ColumnCount-1|] |> Array.map(fun _ ->  100.0)
        let constraints = [0 .. A.ColumnCount-1]
                            |> Seq.map(fun i -> getConstraint i )
                            |> Seq.collect(fun x -> x)
                            
        
        //First solver on Logs scale
        let solver = new AugmentedLagrangian(f,constraints)
        
        
        (solver.Optimizer :?> BroydenFletcherGoldfarbShanno).Delta <- 1e-4
        (solver.Optimizer :?> BroydenFletcherGoldfarbShanno).FunctionTolerance <- 1e-1
        (solver.Optimizer :?> BroydenFletcherGoldfarbShanno).Epsilon <- 1e-2
        solver.MaxEvaluations <- 100

        
        
        solver.Token<-token.Token
        

        if solver.Maximize(data) then        
            solver.Solution
        else
            failwith ""


    let Optimize(q:double,A:Matrix<float>,Pk:Vector<float>,ck:Vector<float>)=  
        
        //Fist apply log optimization
        Optimize_log(q,A,Pk,ck)
        