using System;
using System.Collections.Generic;

namespace MetricCollector.Model
{
    public class ElasticSearchModel
    {
        public int took {get;set;}
        public bool timed_out {get;set;}
        public Shard _shards {get;set;}
        public OuterHit hits{get;set;}
    }

    public class OuterHit
    {
        public Total total {get;set;}
        public double max_score {get;set;}
        public Hit[] hits {get;set;}
    }

    public class Total
    {
        public int Value {get;set;}
        public string relation {get;set;}
    }

    public class Shard
    {
        public int total {set;get;}
        public int successful {set;get;}
        public int skipped {set;get;}
        public int failed {set;get;}
    }
    public class Hit
    {
        public string _index {get;set;}
        public string _type {get;set;}
        public string _id {get;set;}
        public double _score {get;set;}
        public Source _source {get;set; }
    }

    public class Source
    {
          public string log {get;set;}
          public string stream {get;set;}
          public Docker docker {get;set;}
          public Kubernetes kubernetes {get;set;}
          public DateTime Timestamp {get;set;}
          public string tag {get;set;}
    }

    public class Docker
    {
        public string container_id {get;set;}

    }


    public class Kubernetes
    {
        public string container_name {get;set;}
        public string namespace_name {get;set;}
        public string pod_name {get;set;}
        public string container_image {get;set;}
        public string container_image_id {get;set;}
        public string pod_id {get;set;}
        public Labels labels  {get;set;}
        public string host {get;set;}
        public string master_url {get;set;}
        public string namespace_id {get;set;}
    }
    public class Labels
    {
        public string app  {get;set;}
    }
}