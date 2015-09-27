using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindEnLejlighed.Services.Domain
{

    public class DatalayerHelper
    {
        public P p { get; set; }
        public A a { get; set; }
        public D d { get; set; }
        public C c { get; set; }
        public L l { get; set; }
        public U1 u { get; set; }
    }

    public class P
    {
        public string t { get; set; }
        public string pl { get; set; }
        public string v { get; set; }
    }

    public class A
    {
        public int id { get; set; }
        public string t { get; set; }
        public int cdt { get; set; }
        public int lpdt { get; set; }
        public int dl { get; set; }
        public int ic { get; set; }
        public Prc prc { get; set; }
        public string[] ftr { get; set; }
        public U u { get; set; }
        public Attr attr { get; set; }
    }

    public class Prc
    {
        public int amt { get; set; }
        public string cur { get; set; }
    }

    public class U
    {
        public bool li { get; set; }
        public string huid { get; set; }
        public string hue { get; set; }
    }

    public class Attr
    {
        public string Propertytype { get; set; }
        public string CityRegion { get; set; }
        public string Minnumberofrooms { get; set; }
        public string minlivingspace { get; set; }
        public string RentalPeriod { get; set; }
        public string TakeoverDate { get; set; }
        public string SELLER_TYPE { get; set; }
    }

    public class D
    {
        public string ck { get; set; }
        public string s_ck { get; set; }
    }

    public class C
    {
        public L1 l1 { get; set; }
        public L2 l2 { get; set; }
        public L3 l3 { get; set; }
        public C1 c { get; set; }
    }

    public class L1
    {
        public int id { get; set; }
        public string n { get; set; }
    }

    public class L2
    {
        public int id { get; set; }
        public string n { get; set; }
    }

    public class L3
    {
        public int id { get; set; }
        public string n { get; set; }
    }

    public class C1
    {
        public int id { get; set; }
        public string n { get; set; }
    }

    public class L
    {
        public C2 c { get; set; }
        public L11 l1 { get; set; }
        public L21 l2 { get; set; }
        public L31 l3 { get; set; }
        public L4 l4 { get; set; }
        public string pcid { get; set; }
        public string ltlng { get; set; }
    }

    public class C2
    {
        public int id { get; set; }
        public string n { get; set; }
    }

    public class L11
    {
        public int id { get; set; }
        public string n { get; set; }
    }

    public class L21
    {
        public int id { get; set; }
        public string n { get; set; }
    }

    public class L31
    {
        public int id { get; set; }
        public string n { get; set; }
    }

    public class L4
    {
        public int id { get; set; }
        public string n { get; set; }
    }

    public class U1
    {
        public bool li { get; set; }
    }

}
