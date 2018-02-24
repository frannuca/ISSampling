namespace ISSampling.EfficientFrontier

module Functions=
    open System
    open MathNet.Numerics.LinearAlgebra
    open System.Data
    open Deedle
    open Deedle.Series
    open System.Numerics
    

    let ComputeRC(x:Vector<float>,R:Matrix<float>)=
        let total2 = x.ToRowMatrix()*R*x.ToColumnMatrix()
        let total = Math.Sqrt(total2.[0,0])
        (x.ToColumnMatrix().PointwiseMultiply(R*x.ToColumnMatrix())).Column(0)/total

    let ComputeReturns(x:Vector<float>,R:Matrix<float>,w:Vector<float>,roe:Vector<float>)=
        let y = x.PointwiseMultiply(w)
        let rc = ComputeRC(y,R)
        let divret = rc.PointwiseMultiply(roe)
        divret

    let MonteCarlo(names:string[],erc0:Vector<float>,nsims:int,sigma:float,R:Matrix<float>,roe:Vector<float>)=
        //generate all transfers:
        let shifts= Matrix<float>.Build.Random(nsims,R.ColumnCount,new MathNet.Numerics.Distributions.ContinuousUniform(1.0-sigma,1.0+sigma))
        for i in 0 .. nsims-1 do
            let row = shifts.Row(i)            
            shifts.SetRow(i,row/row.Sum()*float(row.Count))

        let ret = Matrix<float>.Build.Dense(nsims,R.ColumnCount)

        let simblock =
            [0 .. shifts.RowCount-1]
            |> Seq.map(fun n ->
                                let t = shifts.Row(n).ToArray()
                                let erct = erc0.PointwiseMultiply(shifts.Row(n)).ToArray()
                                let r = ComputeReturns(erc0,R,shifts.Row(n),roe).ToArray()
                                let s1= names 
                                        |> Seq.mapi(fun i name -> n,"shift_"+name,t.[i])
                                let s2= names 
                                        |> Seq.mapi(fun i name -> n,"erc_"+name,erct.[i])
                                
                                let s3= names 
                                        |> Seq.mapi(fun i name -> n,"returns_"+name,r.[i])
                                s1,s2,s3
                                )
            |> Seq.fold(fun s (a,b,c) ->
                                            let aux  = [|a;b;c|] 
                                            Array.concat([s;aux]))([||])
            
            |> Seq.collect(fun h -> h)
            |> Frame.ofValues
            
        
        simblock
