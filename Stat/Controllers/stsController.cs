using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Stat.Models;
using System.Messaging;

namespace Stat.Controllers
{
    [RoutePrefix("stat")]
    public class stsController : ApiController
    {

        public stsController()
        {
            Task.Run(() => backwork());
        }

        private async void backwork()
        {
            StatContext db2 = new StatContext();
            MessageQueue queue;
            if (MessageQueue.Exists(@".\private$\InStat"))
            {
                queue = new MessageQueue(@".\private$\InStat");
            }
            else
            {
                queue = MessageQueue.Create(".\\private$\\InStat");
            }

            MessageQueue queue2;
            if (MessageQueue.Exists(@".\private$\OutStat"))
            {
                queue2 = new MessageQueue(@".\private$\OutStat");
            }
            else
            {
                queue2 = MessageQueue.Create(".\\private$\\OutStat");
            }
            
            using (queue)
            {
                stat sss = new stat();
                instatmes qqq = new instatmes();
                outstatmes ddd2 = new outstatmes();
                Message ret2 = new Message();
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(instatmes) });
                queue2.Formatter = new XmlMessageFormatter(new Type[] { typeof(outstatmes) });
                while (queue.CanRead)
                {
                    Message m = queue.Receive();
                    
                    qqq = (instatmes)m.Body;

                    sss.request_type = qqq.request_type;
                    sss.server_name = qqq.server_name;
                    sss.Time = qqq.Time;
                    sss.detail = qqq.detail;
                    sss.state = qqq.state;                 

                    try
                    {
                        db2.stats.Add(sss);
                        await db2.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        outstatmes ddd = new outstatmes();
                        ddd.status = -1;
                        ddd.Error = "Bad data.";
                        ddd.mes = qqq;

                        Message ret = new Message(ddd);
                        switch (ddd.mes.server_name)
                        {
                            case server_name.AUTH:
                                ret.Label = "ANSAUTH";
                                break;
                            case server_name.GATE:
                                ret.Label = "ANGATE";
                                break;
                            case server_name.COMP:
                                ret.Label = "ANCOMP";
                                break;
                            case server_name.PERS:
                                ret.Label = "ANPERS";
                                break;
                            case server_name.REG:
                                ret.Label = "ANREG";
                                break;
                        }

                        queue2.Send(ret);
                        continue;   
                    }

                    
                    ddd2.status = 0;
                    ddd2.Error = "";
                    ddd2.mes = qqq;

                    ret2.Body = ddd2;
                    switch (ddd2.mes.server_name)
                    {
                        case server_name.AUTH:
                            ret2.Label = "ANSAUTH";
                            break;
                        case server_name.GATE:
                            ret2.Label = "ANGATE";
                            break;
                        case server_name.COMP:
                            ret2.Label = "ANCOMP";
                            break;
                        case server_name.PERS:
                            ret2.Label = "ANPERS";
                            break;
                        case server_name.REG:
                            ret2.Label = "ANREG";
                            break;
                    }

                    queue2.Send(ret2);
                }
            }
            
        }

        private gateinf scangate(StatContext dd)
        {
            var sss = dd.stats.Where(x => x.server_name == server_name.GATE).ToList();
            int k = 0, l = 0;
            List<int> mass = new List<int>(24);
            for (int i = 0; i < 24; i++)
            {
                mass.Add(0);
            }
            List<int> mass2 = new List<int>(3);
            for (int i = 0; i < 3; i++)
            {
                mass2.Add(0);
            }
            //int[] mass = new int[24];
            //int[] mass2 = new int[3];
            foreach (var item in sss)
            {
                string q = item.detail.ToString();
                string[] w = q.Split(' ');
                if (w[0] == "UNAUTHORIZED")
                {
                    k++;
                }
                if (w[0] == "ACCESS")
                {
                    l++;
                }

                mass[item.Time.Value.Hour]++;
                mass2[(int)item.request_type]++;

            }
            gateinf ff = new gateinf();
            ff.anauth = k;
            ff.auth = l;
            ff.rasp = mass;
            ff.resp2 = mass2;

            return ff;
        }

        private miniinf scancomp(StatContext dd)
        {
            var sss = dd.stats.Where(x => x.server_name == server_name.COMP).ToList();

            List<int> mass = new List<int>(24);
            for (int i = 0; i < 24; i++)
            {
                mass.Add(0);
            }
            List<int> mass2 = new List<int>(3);
            for (int i = 0; i < 3; i++)
            {
                mass2.Add(0);
            }
            //int[] mass = new int[24];
            //int[] mass2 = new int[3];
            foreach (var item in sss)
            {
                string q = item.detail.ToString();
                string[] w = q.Split(' ');
                if (w[0] == "PUT")
                {
                    mass2[0]++;
                }
                if (w[0] == "POST")
                {
                    mass2[1]++;
                }
                if (w[0] == "DELETE")
                {
                    mass2[2]++;
                }

                mass[item.Time.Value.Hour]++;

            }
            miniinf ff = new miniinf();
            ff.rasp = mass;
            ff.resp2 = mass2;

            return ff;
        }

        private miniinf scanpers(StatContext dd)
        {
            var sss = dd.stats.Where(x => x.server_name == server_name.PERS).ToList();
            List<int> mass = new List<int>(24);
            for (int i = 0; i < 24; i++)
            {
                mass.Add(0);
            }
            List<int> mass2 = new List<int>(3);
            for (int i = 0; i < 3; i++)
            {
                mass2.Add(0);
            }
            //int[] mass = new int[24];
            //int[] mass2 = new int[3];
            foreach (var item in sss)
            {
                string q = item.detail.ToString();
                string[] w = q.Split(' ');
                if (w[0] == "PUT")
                {
                    mass2[0]++;
                }
                if (w[0] == "POST")
                {
                    mass2[1]++;
                }
                if (w[0] == "DELETE")
                {
                    mass2[2]++;
                }

                mass[item.Time.Value.Hour]++;

            }
            miniinf ff = new miniinf();
            ff.rasp = mass;
            ff.resp2 = mass2;

            return ff;
        }

        private miniinf scanreg(StatContext dd)
        {
            var sss = dd.stats.Where(x => x.server_name == server_name.REG).ToList();
            List<int> mass = new List<int>(24);
            for (int i = 0; i < 24; i++)
            {
                mass.Add(0);
            }
            List<int> mass2 = new List<int>(3);
            for (int i = 0; i < 3; i++)
            {
                mass2.Add(0);
            }
            //int[] mass = new int[24];
            //int[] mass2 = new int[3];
            foreach (var item in sss)
            {
                string q = item.detail.ToString();
                string[] w = q.Split(' ');
                if (w[0] == "PUT")
                {
                    mass2[0] +=1;
                }
                if (w[0] == "POST")
                {
                    mass2[1]++;
                }
                if (w[0] == "DELETE")
                {
                    mass2[2]++;
                }

                mass[item.Time.Value.Hour]++;

            }
            miniinf ff = new miniinf();
            ff.rasp = mass;
            ff.resp2 = mass2;

            return ff;
        }

        [Route("all")]
        public async Task<IHttpActionResult> GetFirst()
        {
            StatContext dbd = new StatContext();
            var fff = scangate(dbd);
            var ggg = scancomp(dbd);
            var hhh = scanpers(dbd);
            var jjj = scanreg(dbd);

            statinf eee = new statinf();
            eee.gateinf1 = fff;
            eee.miniinf1 = ggg;
            eee.miniinf2 = hhh;
            eee.miniinf3 = jjj;

            return Ok<statinf>(eee);
        }
        
        [Route("start")]
        public IHttpActionResult Get()
        {
            return Ok();
        }
    }
}