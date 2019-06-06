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
    public class TaloValoController : Controller
    {
        // GET: TaloValo
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetList()
        {
            //Luodaan uusi entiteettiolio 
            ProjektitEntities entities = new ProjektitEntities();

            //Haetaan TaloValo -taulusta kaikki data
            var valot = (from val in entities.TaloValo
                         join tal in entities.Talot
                         on val.TaloId equals tal.TaloId
                          select new
                          {
                              val.ValoId,
                              val.TaloId,
                              tal.TaloNimi,
                              val.Huone,
                              val.ValoNimi,
                              val.ValoMaara,
                              val.ValoTila
                          }).ToList();

            //Muutetaan data json -muotoon toimitettavaksi selaimelle. 
            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            string json = JsonConvert.SerializeObject(valot, serializerSettings);

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

            //Muutetaan modaali-ikkunasta tullut string-tyyppinen ValoId integeriksi
            int valID = int.Parse(id);

            //Haetaan TaloValo -taulusta kaikki data
            var valot = (from val in entities.TaloValo
                          where val.ValoId == valID
                          select val).FirstOrDefault();

            //Muutetaan olio json -muotoon toimitettavaksi selaimelle. Suljetaan tietokantayhteys.
            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };

            string json = JsonConvert.SerializeObject(valot, serializerSettings);
            entities.Dispose();

            //ohitetaan välimuisti, jotta näyttö päivittyy (IE-selainta varten) 
            Response.Expires = -1;
            Response.CacheControl = "no-cache";

            //Lähetetään data selaimelle
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(TaloValo valot)
        {
            //Tietojen päivitys ja uuden valon lisäys

            bool OK = false;    //tallennuksen onnistuminen

            //tietokantaan tallennetaan uusia tietoja vain, mikäli Huone ja ValoNimi -kentät ei ole tyhjä
            if (!string.IsNullOrWhiteSpace(valot.Huone) &&
                    !string.IsNullOrWhiteSpace(valot.ValoNimi))
            {
                //luodaan uusi entiteettiolio
                ProjektitEntities entities = new ProjektitEntities();

                int valoid = valot.ValoId;

                if (valoid == 0)
                {
                    //Uuden Valon lisääminen tietokantaan dbItem-nimisen olion avulla
                    TaloValo dbItem = new TaloValo()
                    {
                        //dbItemin arvot/tiedot
                        TaloId = valot.TaloId,
                        Huone = valot.Huone,
                        ValoNimi = valot.ValoNimi,
                        ValoMaara = valot.ValoMaara,
                        ValoTila = valot.ValoTila
                    };

                    //lisätään tietokantaan dbItemin tiedot ja tallennetaan muutokset
                    entities.TaloValo.Add(dbItem);
                    entities.SaveChanges();
                    OK = true;
                }
                else
                {
                    //muokataan olemassa olevia tietoja ja haetaan tiedot tietokannasta

                    TaloValo dbItem = (from val in entities.TaloValo
                                        where val.ValoId == valoid
                                        select val).FirstOrDefault();

                    //tallennetaan modaali-ikkunasta tulevat tiedot dbItem-olioon
                    if (dbItem != null)
                    {
                        dbItem.TaloId = valot.TaloId;
                        dbItem.Huone = valot.Huone;
                        dbItem.ValoNimi = valot.ValoNimi;
                        dbItem.ValoMaara = valot.ValoMaara;
                        dbItem.ValoTila = valot.ValoTila;

                        //tallennetaan uudet tiedot tietokantaan
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

        //valon poisto
        public ActionResult Delete(string id)
        {
            ProjektitEntities entities = new ProjektitEntities();

            //etsitään id:n perusteella valorivi kannasta
            bool OK = false;

            int valoid = int.Parse(id);

            TaloValo dbItem = (from val in entities.TaloValo
                                where val.ValoId == valoid 
                                select val).FirstOrDefault();

            //tallennetaan modaali-ikkunasta tulevat tiedot dbItem-olioon
            if (dbItem != null)
            {
                //tietokannasta poisto
                entities.TaloValo.Remove(dbItem);
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
