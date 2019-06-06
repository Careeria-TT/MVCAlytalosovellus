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
    public class TalotController : Controller
    {
        // GET: Talot
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetList()
        {
            //Luodaan uusi entiteettiolio 
            ProjektitEntities entities = new ProjektitEntities();

            //Haetaan Talot -taulusta kaikki data
            var talot = (from tal in entities.Talot
                          select new
                          {
                              tal.TaloId,
                              tal.TaloNimi
                          }).ToList();

            //Muutetaan data json -muotoon toimitettavaksi selaimelle. 
            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            string json = JsonConvert.SerializeObject(talot, serializerSettings);

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

            //Muutetaan modaali-ikkunasta tullut string-tyyppinen TaloId int-tyyppiseksi
            int talID = int.Parse(id);

            //Haetaan Talot -taulusta kaikki data
            var talot = (from tal in entities.Talot
                          where tal.TaloId == talID
                          select tal).FirstOrDefault();

            //Muutetaan olio json -muotoon toimitettavaksi selaimelle. Suljetaan tietokantayhteys.
            var serializerSettings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects };

            string json = JsonConvert.SerializeObject(talot, serializerSettings);
            entities.Dispose();

            //ohitetaan välimuisti, jotta näyttö päivittyy (IE-selainta varten) 
            Response.Expires = -1;
            Response.CacheControl = "no-cache";

            //Lähetetään data selaimelle
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(Talot talot)
        {
            //Tietojen päivitys ja uuden talon lisäys

            bool OK = false;    //tallennuksen onnistuminen

            //tietokantaan tallennetaan uusia tietoja vain, mikäli talon nimi -kenttä ei ole tyhjä
            if (!string.IsNullOrWhiteSpace(talot.TaloNimi))
            {
                //luodaan uusi entiteettiolio
                ProjektitEntities entities = new ProjektitEntities();

                int taloid = talot.TaloId;

                if (taloid == 0)
                {
                    //Uuden talon lisääminen tietokantaan dbItem-nimisen olion avulla
                    Talot dbItem = new Talot()
                    {
                        //dbItemin arvot/tiedot
                        TaloNimi = talot.TaloNimi
                    };

                    //lisätään tietokantaan dbItemin tiedot ja tallennetaan muutokset
                    entities.Talot.Add(dbItem);
                    entities.SaveChanges();
                    OK = true;
                }
                else
                {
                    //muokataan olemassa olevia tietoja
                    //haetaan tiedot tietokannasta

                    Talot dbItem = (from tal in entities.Talot
                                        where tal.TaloId == taloid
                                        select tal).FirstOrDefault();

                    //tallennetaan modaali-ikkunasta tulevat tiedot dbItem-olioon
                    if (dbItem != null)
                    {
                        dbItem.TaloNimi = talot.TaloNimi;

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

        //Talon poisto
        public ActionResult Delete(string id)
        {
            ProjektitEntities entities = new ProjektitEntities();
            
            // etsitään id:n perusteella talorivi kannasta
            bool OK = false;

            int taloid = int.Parse(id);

            Talot dbItem = (from tal in entities.Talot
                                where tal.TaloId == taloid
                                select tal).FirstOrDefault();

            //tallennetaan modaali-ikkunasta tulevat tiedot dbItem-olioon
            if (dbItem != null)
            {
                //tietokannasta poisto
                entities.Talot.Remove(dbItem);
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






