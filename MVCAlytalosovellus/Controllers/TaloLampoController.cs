using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVCAlytalosovellus.Models;
using Newtonsoft.Json;


namespace MVCAlytalosovellus.Controllers
{
    public class TaloLampoController : Controller
    {
        // GET: TaloLampo
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetList()
        {
            //Luodaan uusi entiteettiolio 
            ProjektitEntities entities = new ProjektitEntities();

            //Haetaan TaloLampo -taulusta kaikki data
            var lampo = (from lam in entities.TaloLampo
                         join tal in entities.Talot
                         on lam.TaloId equals tal.TaloId
                         select new
                          {
                              lam.LampoId,
                              lam.TaloId,
                              tal.TaloNimi,
                              lam.Huone,
                              lam.HuoneTavoiteLampotila,
                              lam.HuoneNykyLampotila
                          }).ToList();

            //Muutetaan data json -muotoon toimitettavaksi selaimelle. 
            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            string json = JsonConvert.SerializeObject(lampo, serializerSettings);

            //Suljetaan tietokantayhteys.
            entities.Dispose();

            //ohitetaan välimuisti, jotta näyttö päivittyy (IE-selainta varten) 
            Response.Expires = -1;
            Response.CacheControl = "no-cache";

            //Lähetetään data selaimelle
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSingleGroup(string id)
        {
            //Luodaan uusi entiteettiolio 
            ProjektitEntities entities = new ProjektitEntities();

            //Muutetaan modaali-ikkunasta tullut string-tyyppinen LampoId integeriksi
            int lamID = int.Parse(id);

            //Haetaan TaloLampo -taulusta kaikki data
            var lampo = (from lam in entities.TaloLampo
                          where lam.LampoId == lamID
                          select lam).FirstOrDefault();

            //Muutetaan olio json -muotoon toimitettavaksi selaimelle. Suljetaan tietokantayhteys.
            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };

            string json = JsonConvert.SerializeObject(lampo, serializerSettings);
            entities.Dispose();

            //ohitetaan välimuisti, jotta näyttö päivittyy (IE-selainta varten) 
            Response.Expires = -1;
            Response.CacheControl = "no-cache";

            //Lähetetään data selaimelle
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(TaloLampo lampo)
        {
            //Tietojen päivitys ja uuden lämmön lisäys

            bool OK = false;    //tallennuksen onnistuminen

            //tietokantaan tallennetaan uusia tietoja vain, mikäli Huone -kenttä ei ole tyhjä
            if (!string.IsNullOrWhiteSpace(lampo.Huone))
            {
                //luodaan uusi entiteettiolio
                ProjektitEntities entities = new ProjektitEntities();

                int lampoid = lampo.LampoId;

                if (lampoid == 0)
                {
                    //Uuden Lämpötilan lisääminen tietokantaan dbItem-nimisen olion avulla
                    TaloLampo dbItem = new TaloLampo()
                    {
                        //dbItemin arvot/tiedot
                        TaloId = lampo.TaloId,
                        Huone = lampo.Huone,
                        HuoneTavoiteLampotila = lampo.HuoneTavoiteLampotila,
                        HuoneNykyLampotila = lampo.HuoneNykyLampotila
                    };

                    //lisätään tietokantaan dbItemin tiedot ja tallennetaan muutokset
                    entities.TaloLampo.Add(dbItem);
                    entities.SaveChanges();
                    OK = true;
                }
                else
                {
                    //muokataan olemassa olevia tietoja ja haetaan tiedot tietokannasta

                    TaloLampo dbItem = (from lam in entities.TaloLampo
                                        where lam.LampoId == lampoid
                                        select lam).FirstOrDefault();

                    //tallennetaan modaali-ikkunasta tulevat tiedot dbItem-olioon
                    if (dbItem != null)
                    {
                        dbItem.TaloId = lampo.TaloId;
                        dbItem.Huone = lampo.Huone;
                        dbItem.HuoneTavoiteLampotila = lampo.HuoneTavoiteLampotila;
                        dbItem.HuoneNykyLampotila = lampo.HuoneNykyLampotila;

                        // tallennetaan uudet tiedot tietokantaan
                        entities.SaveChanges();
                        OK = true;
                    }
                }
                //suljetaan tietokantayhteys
                entities.Dispose();
            }

            //palautetaan tallennuskuittaus selaimelle (muuttuja OK)
            return Json(OK, JsonRequestBehavior.AllowGet);
        }

        //Lämpötilan poisto
        public ActionResult Delete(string id)
        {
            ProjektitEntities entities = new ProjektitEntities();

            // etsitään id:n perusteella lämpötilarivi kannasta
            bool OK = false;

            int lampoid = int.Parse(id);

            TaloLampo dbItem = (from lam in entities.TaloLampo
                                where lam.LampoId == lampoid
                                select lam).FirstOrDefault();

            //tallennetaan modaali-ikkunasta tulevat tiedot dbItem-olioon
            if (dbItem != null)
            {
                //tietokannasta poisto
                entities.TaloLampo.Remove(dbItem);
                entities.SaveChanges();
                OK = true;
            }
            //suljetaan tietokantayhteys
            entities.Dispose();

            //palautetaan tallennuskuittaus selaimelle (muuttuja OK)
            return Json(OK, JsonRequestBehavior.AllowGet);
        }
    }
}
