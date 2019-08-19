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
            vnetSubnetId: string
            maxPods: string
            workspaceResourceId: string
            nodepoolName: string
            dnsServiceIP: string
            serviceCIDR: string
            dockerBridgeCIDR: string
            networkPlugin: string
            dnsNamePrefix: string
        }

    let optional propName propValue =
        match propValue with
                | "" -> ""
                | _ -> sprintf " %s %s" propName propValue

    let createK8sCluster cluster = 
        let vnetSubnetId = optional "--vnet-subnet-id" cluster.vnetSubnetId
        let maxPods = optional "--max-pods" cluster.maxPods
        let workspaceResourceId = optional "--workspace-resource-id" cluster.workspaceResourceId
        let nodepoolName = optional "--nodepool-name" cluster.nodepoolName
        let dnsServiceIP = optional "--dns-service-ip" cluster.dnsServiceIP
        let serviceCIDR = optional "--service-cidr" cluster.serviceCIDR
        let dockerBridgeCIDR = optional "--docker-bridge-address" cluster.dockerBridgeCIDR
        let networkPlugin = optional "--network-plugin" cluster.networkPlugin
        let dnsNamePrefix = optional "--dns-name-prefix" cluster.dnsNamePrefix

        az (
            sprintf 
                "aks create -g %s -n %s --service-principal %s --client-secret %s --kubernetes-version %s --node-count %i --node-vm-size %s --enable-addons %s --generate-ssh-keys%s%s%s%s%s%s%s%s%s" 
                cluster.resourceGroup
                cluster.clusterName
                cluster.servicePrincipalId
                cluster.servicePrincipalSecret
                cluster.k8sVersion
                cluster.nodeCount
                cluster.nodeVmSize
                cluster.enableAddons
                vnetSubnetId
                maxPods
                workspaceResourceId
                nodepoolName
                dnsServiceIP
                serviceCIDR
                dockerBridgeCIDR
                networkPlugin
                dnsNamePrefix
        ) |> ignore

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
        let modifiedFile = "namespace.yml"
        (sprintf "apiVersion: v1
kind: Namespace
metadata:
  name: %s" namespaceName)
        |> File.replaceContent modifiedFile

        kubectl [|"apply";"-f";modifiedFile|] |> ignore
        File.delete(modifiedFile)
    
    let setQuotas quotas namespaceName= 
        let modifiedFile = "quotas.yml"
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

        