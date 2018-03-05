namespace ISSampling.EfficientFrontier

open MathNet.Numerics.LinearAlgebra
open FSharp.Data.HtmlAttribute
open GAF
open GAF.Operators
open System

module ClusterOptimizer=
    open HierarchyOptimizer

    let ComputeReturns(root:Node<NodeInfo>,rframe:Correlation)(weights:float Vector)=
        let nodes = root.Children.ToArray() |> Array.map(fun x -> x.Name,x) |> dict
        
        let nodeinfos = rframe.Columns |> Array.map(fun x -> nodes.[x]) |> Array.ofSeq 
        let erc = nodeinfos |> Seq.map(fun x -> x.Data.ERC) |> Array.ofSeq
        let roes = nodeinfos |> Seq.map(fun x -> x.Data.RoE) |> Array.ofSeq
        let roe  = Vector<float>.Build.DenseOfArray roes
        let x = Vector<float>.Build.DenseOfArray erc
        let totalERC2:Matrix<float> = x.ToRowMatrix()*rframe.R*x.ToColumnMatrix()
        let totalERC = Math.Sqrt (totalERC2.[0,0])
        let rc = x.ToColumnMatrix().PointwiseMultiply(rframe.R*x.ToColumnMatrix())/totalERC
        rc.Column(0).PointwiseMultiply(weights).PointwiseMultiply(roe).Sum()

    let Fitness(root:Node<NodeInfo>,rframe:Correlation,nparams:int,minval:float,maxval:float)(chromosome:Chromosome)=
        
        let nbits = chromosome.Count
        
        let bitperx = nbits/nparams
        let baseval = Math.Pow(2.0,float(bitperx))

        let w = [0 .. nparams-1] 
                |> Seq.map(fun i ->  Convert.ToInt32(chromosome.ToBinaryString(i*bitperx, bitperx),2))
                |> Seq.map(fun v -> minval+ (maxval-minval)*float(v)/baseval)
                |> Array.ofSeq

        ComputeReturns(root,rframe)(w)
        
        
    let OptimizeSubTree(root:Node<NodeInfo>,R:Correlation,minTransfer:float,maxTransfer:float)=
        
        let nparams = root.Children.Count
        let nbittperParam = 7
        let nbits = nparams*nbittperParam

        //Genetic Algorithm Builder
        let cossover = new Crossover(0.8,true,CrossoverType.DoublePoint)
        let mutation = new BinaryMutate(0.01,true)
        let elitism = new Elite(10)
        let parentSelection = ParentSelectionMethod.FitnessProportionateSelection


        let PopulationSize = 500
        let population = new Population(PopulationSize,nbits)
        let funcc =  Fitness(root,R,nparams,minTransfer,maxTransfer)
        
        let f_func = Fitness(root,R,nparams,minTransfer,maxTransfer)
            
        let uax = System.Func<Chromosome,float>(f_func)
        let f = fun x -> f_func(x)
        let yyy = new FitnessFunction(f)
        let ga = new GeneticAlgorithm(population,yyy)
        ga.Operators.Add(cossover)
        ga.Operators.Add(mutation)
        ga.Operators.Add(elitism)
      

        ga

    let Optimize(Rs:System.Collections.Generic.IDictionary<string,Correlation>,root:Node<NodeInfo>,minTransfer:float,maxTransfer:float)=
        
        //extracting paramter for levels to optimize:

        let nodes = root.BreadthFirst |> Seq.rev |> Array.ofSeq
        let levels = nodes |> Seq.map(fun n -> n.Depth) |> Seq.distinct |> Seq.sort |> Array.ofSeq
        let toplevel = levels.[0]
        let bottomlevel = levels.[levels.Length-1]

        
        failwith ""
    


