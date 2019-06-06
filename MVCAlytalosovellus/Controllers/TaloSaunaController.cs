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
    public class TaloSaunaController : Controller
    {
        // GET: TaloSauna
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetList()
        {
            //Luodaan uusi entiteettiolio 
            ProjektitEntities entities = new ProjektitEntities();

            //Haetaan TaloSauna -taulusta kaikki data
            var saunat = (from sau in entities.TaloSauna
                          join tal in entities.Talot
                          on sau.TaloId equals tal.TaloId
                          select new
                             {
                                 sau.SaunaId,
                                 sau.TaloId,
                                 tal.TaloNimi,
                                 sau.SaunaNimi,
                                 sau.SaunaNykyLampotila,
                                 sau.SaunaTila
                             }).ToList();

            //Muutetaan data json -muotoon toimitettavaksi selaimelle. 
            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            string json = JsonConvert.SerializeObject(saunat, serializerSettings);

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

            //Muutetaan modaali-ikkunasta tullut string-tyyppinen SaunaId int-tyyppiseksi
            int sauID = int.Parse(id);

            //Haetaan TaloSauna -taulusta kaikki data
            var saunat = (from sau in entities.TaloSauna
                           where sau.SaunaId == sauID
                           select sau).FirstOrDefault();

            //Muutetaan olio json -muotoon toimitettavaksi selaimelle. Suljetaan tietokantayhteys.
            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };

            string json = JsonConvert.SerializeObject(saunat, serializerSettings);
            entities.Dispose();

            //ohitetaan välimuisti, jotta näyttö päivittyy (IE-selainta varten) 
            Response.Expires = -1;
            Response.CacheControl = "no-cache";

            //Lähetetään data selaimelle
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(TaloSauna saunat)
        {
            //Tietojen päivitys ja uuden saunan lisäys

            bool OK = false;    //tallennuksen onnistuminen

            //tietokantaan tallennetaan uusia tietoja vain, mikäli saunan nimi -kenttä ei ole tyhjä
            if (!string.IsNullOrWhiteSpace(saunat.SaunaNimi))
            {
                //luodaan uusi entiteettiolio
                ProjektitEntities entities = new ProjektitEntities();

                int saunaid = saunat.SaunaId;

                if (saunaid == 0)
                {
                    //Uuden Saunan lisääminen tietokantaan dbItem-nimisen olion avulla
                    TaloSauna dbItem = new TaloSauna()
                    {
                        //dbItemin arvot/tiedot
                        TaloId = saunat.TaloId,
                        SaunaNimi = saunat.SaunaNimi,
                        SaunaNykyLampotila = saunat.SaunaNykyLampotila,
                        SaunaTila = saunat.SaunaTila
                    };

                    //lisätään tietokantaan dbItemin tiedot ja tallennetaan muutokset
                    entities.TaloSauna.Add(dbItem);
                    entities.SaveChanges();
                    OK = true;
                }
                else
                {
                    //muokataan olemassa olevia tietoja
                    //haetaan tiedot tietokannasta

                    TaloSauna dbItem = (from sau in entities.TaloSauna
                                        where sau.SaunaId == saunaid
                                        select sau).FirstOrDefault();

                    //tallennetaan modaali-ikkunasta tulevat tiedot dbItem-olioon
                    if (dbItem != null)
                    {
                        dbItem.TaloId = saunat.TaloId;
                        dbItem.SaunaNimi = saunat.SaunaNimi;
                        dbItem.SaunaNykyLampotila = saunat.SaunaNykyLampotila;
                        dbItem.SaunaTila = saunat.SaunaTila;

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

        //Saunan poisto
        public ActionResult Delete(string id)
        {
            ProjektitEntities entities = new ProjektitEntities();

            // etsitään id:n perusteella saunarivi kannasta
            bool OK = false;

            int saunaid = int.Parse(id);

            TaloSauna dbItem = (from sau in entities.TaloSauna
                                where sau.SaunaId == saunaid
                                select sau).FirstOrDefault();

            //tallennetaan modaali-ikkunasta tulevat tiedot dbItem-olioon
            if (dbItem != null)
            {
                //tietokannasta poisto
                entities.TaloSauna.Remove(dbItem);
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
