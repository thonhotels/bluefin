namespace Bluefin

open Bluefin.Core
open Bluefin.Kube
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
        }

    let getOptionalClusterProp propName propValue =
        match propValue with
                | "" -> ""
                | _ -> sprintf " %s %s" propName propValue

    let createK8sCluster cluster = 
        let vnetSubnetId = getOptionalClusterProp "--vnet-subnet-id" cluster.vnetSubnetId
        let maxPods = getOptionalClusterProp "--max-pods" cluster.maxPods
        let workspaceResourceId = getOptionalClusterProp "--workspace-resource-id" cluster.workspaceResourceId
        let nodepoolName = getOptionalClusterProp "--nodepool-name" cluster.nodepoolName

        az (
            sprintf 
                "aks create -g %s -n %s --service-principal %s --client-secret %s --kubernetes-version %s --node-count %i --node-vm-size %s --enable-addons %s --generate-ssh-keys%s%s%s%s" 
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
        ) |> ignore


     --nodepool-name

    let getCredentials resourceGroup clusterName =
        az (sprintf "aks get-credentials -g %s -n %s" resourceGroup clusterName) |> ignore

    let setCluster clusterName =
        kube (sprintf "config set-cluster %s" clusterName)
    
    let createClusterRoleBinding () = 
        try
            kube "create clusterrolebinding kubernetes-dashboard --clusterrole=cluster-admin --serviceaccount=kube-system:kubernetes-dashboard"
        with
        | e when e.Message.Contains("(AlreadyExists)") -> ""
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

        kube (sprintf "apply -f %s" modifiedFile) |> ignore
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

        match namespaceName with
        | "" -> kube (sprintf "apply -f %s" modifiedFile) |> ignore
        | name -> kube (sprintf "apply --namespace %s -f %s" name modifiedFile) |> ignore

        File.delete(modifiedFile)

        