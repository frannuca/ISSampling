namespace ISSampling.EfficientFrontier

open MathNet.Numerics.LinearAlgebra
open FSharp.Data.HtmlAttribute
open GAF
open GAF.Operators

open System
module HierarchyOptimizer=
    open MathNet.Numerics
    
    type Correlation={Columns:string array;R:Matrix<float>}

    type Node<'a>(name:string,data:'a,parent:Node<'a> option)=
        let _name=name
        let _data=data
        let _children:System.Collections.Generic.List<'a Node>=new System.Collections.Generic.List<'a Node>()
        let mutable _parent = parent

        member self.Name = _name
        member self.Parent 
                            with get() = _parent
                            and set(value) = _parent <- value
        
        member self.Children with get() = _children
        member self.Data= _data
                            

        member self.Depth=
            let rec depth(node:Node<'a>,counter:int)=
                match node.Parent with
                        |None -> counter
                        |Some(p) -> depth(p,counter+1)                
            depth(self,0)
        
        member self.BreadthFirst=
            let queue = new System.Collections.Generic.Queue<'a Node>()
            queue.Enqueue(self)
            let nodes = new System.Collections.Generic.List<Node<'a>>()
            while queue.Count > 0 do
                let x = queue.Dequeue()
                nodes.Add(x)
                x.Children |> Seq.iter(queue.Enqueue)
            
            nodes

        member self.DepthFirst=
            let stack = new System.Collections.Generic.Stack<'a Node>()
            stack.Push(self)
            let nodes = new System.Collections.Generic.List<Node<'a>>()
            while stack.Count > 0 do
                let x = stack.Pop()
                nodes.Add(x)
                x.Children |> Seq.iter(stack.Push)
            
            nodes


    type NodeInfo={RoE:float;ERC:float}

    let ExtractNodes(nodes:Node<NodeInfo> array,nodenames:string array)=
        nodenames |> Seq.map(fun nodename -> match nodes |> Seq.filter(fun y -> y.Name=nodename) |> List.ofSeq with
                                                       |[y] -> y.Data
                                                       | _ -> failwith (sprintf "Node name %A is not part of the computation list" nodename                                                       
                                                  )
                                                  )
    