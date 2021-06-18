namespace Bluefin

open Bluefin.Core
open Bluefin.Kubernetes.Core
open Fake.IO

module Aks =

    type Cluster = 
        { 
            resourceGroup: string
            clusterName: string
            k8sVersion: string
            servicePrincipalId: string
            servicePrincipalSecret: string
            nodeCount: int
            nodeVmSize: string
            enableAddons: string
            loadBalancerSku: string
            vnetSubnetId: string
            maxPods: string
            workspaceResourceId: string
            nodepoolName: string
            dnsServiceIP: string
            serviceCIDR: string
            dockerBridgeCIDR: string
            networkPlugin: string
            dnsNamePrefix: string
            loadBalancerOutboundIP: string
        }
        
    let showClientId resourceGroup clusterName =
        az (sprintf "aks show -g %s -n %s --query servicePrincipalProfile.clientId" resourceGroup clusterName)
        
    let createK8sCluster cluster = 
        let baseCmd = [ 
            "aks"; "create"
            "-g"; cluster.resourceGroup
            "-n"; cluster.clusterName
            "--service-principal"; cluster.servicePrincipalId
            "--client-secret"; cluster.servicePrincipalSecret
            "--kubernetes-version"; cluster.k8sVersion
            "--node-count"; cluster.nodeCount.ToString()
            "--node-vm-size"; cluster.nodeVmSize
            "--enable-addons"; cluster.enableAddons
            "--generate-ssh-keys"
        ]

        let optional propName propValue =
            match propValue with
                    | "" -> []
                    | _ -> [propName;propValue] 

        seq {
            yield! baseCmd
            yield! optional "--load-balancer-sku" cluster.loadBalancerSku
            yield! optional "--vnet-subnet-id" cluster.vnetSubnetId
            yield! optional "--max-pods" cluster.maxPods
            yield! optional "--workspace-resource-id" cluster.workspaceResourceId
            yield! optional "--nodepool-name" cluster.nodepoolName
            yield! optional "--dns-service-ip" cluster.dnsServiceIP
            yield! optional "--service-cidr" cluster.serviceCIDR
            yield! optional "--docker-bridge-address" cluster.dockerBridgeCIDR
            yield! optional "--network-plugin" cluster.networkPlugin
            yield! optional "--dns-name-prefix" cluster.dnsNamePrefix
            yield! optional "--load-balancer-outbound-ips" cluster.loadBalancerOutboundIP
        } 
        |> Seq.toArray
        |> azArr
        |> ignore

    let getCredentials resourceGroup clusterName =
        azArr [|"aks";"get-credentials";"-g";resourceGroup;"-n";clusterName;"--overwrite-existing"|] |> ignore

    let setCluster clusterName =
        kubectl [|"config";"set-cluster";clusterName|] |> ignore
    
    let createClusterRoleBinding () = 
        try
            kubectl [|"create";"clusterrolebinding";"kubernetes-dashboard";"--clusterrole=cluster-admin";"--serviceaccount=kube-system:kubernetes-dashboard"|] |> ignore
        with
        | e when e.Message.Contains("(AlreadyExists)") -> ()
        | e -> raise e
    
    type Quotas = {
        name: string
        maxPods: string
        maxRequestsCPU : string
        maxLimitsCPU : string
        maxRequestsMemory : string
        maxLimitsMemory : string
    }

    let createNamespace namespaceName = 
        let modifiedFile = "namespace.yaml"
        (sprintf "apiVersion: v1
kind: Namespace
metadata:
  name: %s" namespaceName)
        |> File.replaceContent modifiedFile

        kubectl [|"apply";"-f";modifiedFile|] |> ignore
        File.delete(modifiedFile)
    
    let setQuotas quotas namespaceName= 
        let modifiedFile = "quotas.yaml"
        (sprintf "apiVersion: v1
kind: ResourceQuota
metadata:
  name: %s
spec:
  hard:
    pods: \"%s\"
    requests.cpu: \"%s\"
    requests.memory: %s
    limits.cpu: \"%s\"
    limits.memory: %s" 
    quotas.name 
    quotas.maxPods 
    quotas.maxRequestsCPU 
    quotas.maxRequestsMemory 
    quotas.maxLimitsCPU 
    quotas.maxLimitsMemory)
        |> File.replaceContent modifiedFile

        let arguments = seq { 
            yield! ["apply";"-f";modifiedFile]
            if namespaceName <> "" then
                yield! ["--namespace";namespaceName]
            }
        kubectl arguments |> ignore

        File.delete(modifiedFile)

        