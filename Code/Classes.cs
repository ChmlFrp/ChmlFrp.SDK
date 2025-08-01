﻿namespace CSDK;

public abstract class Classes // 存储了所有的类定义
{
    public class UserInfoClass
    {
        public string username { get; set; }
        public string usergroup { get; set; }
        public string userimg { get; set; }
        public string usertoken { get; set; }
        public int id { get; set; }
        public string term { get; set; }
        public string qq { get; set; }
        public string email { get; set; }
        public int bandwidth { get; set; }
        public int tunnel { get; set; }
        public int tunnelCount { get; set; }
        public string realname { get; set; }
        public string regtime { get; set; }
        public int integral { get; set; }
    }

    public class TunnelInfoClass
    {
        public int id { get; set; }
        public string ip { get; set; }
        public string dorp { get; set; }
        public string name { get; set; }
        public string node { get; set; }
        public string state { get; set; }
        public string nodestate { get; set; }
        public string type { get; set; }
        public string localip { get; set; }
        public int nport { get; set; }
        public int cur_conns { get; set; }
    }

    public class NodeDataClass
    {
        public long id { get; set; }
        public string name { get; set; }
        public string area { get; set; }
        public string notes { get; set; }
        public string nodegroup { get; set; }
        public string china { get; set; }
        public string web { get; set; }
        public string udp { get; set; }
        public string fangyu { get; set; }
    }

    public class NodeInfoClass : NodeDataClass
    {
        public string ip { get; set; }
        public string location { get; set; }
        public string type { get; set; }
        public string state { get; set; }
        public long uptime_seconds { get; set; }
        public float load15 { get; set; }
        public int users { get; set; }
        public int bandwidth { get; set; }
        public int traffic { get; set; }
        public int port { get; set; }
        public int adminPort { get; set; }
        public long memory_total { get; set; }
        public long storage_total { get; set; }
        public long storage_used { get; set; }
        public float total_traffic_in { get; set; }
        public float total_traffic_out { get; set; }
        public string cpu_info { get; set; }
        public string nodetoken { get; set; }
        public string realIp { get; set; }
        public string rport { get; set; }
        public bool toowhite { get; set; }
    }
}