using Fakturisanje.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace Fakturisanje.Controllers
{
    public class HomeController : Controller
    {
        private FaktureDBEntities _db = new FaktureDBEntities();

        // GET: Home
        public ActionResult Index()
        {
            return View(_db.FakturaTables.ToList());
        }

        // GET: Home/Details/5
        public ActionResult Details(int id)
        {
            var faktura = (from f in _db.FakturaTables

                           where f.fakturaID == id

                           select f).First();

            return View(faktura.StavkaFaktureTables.ToList());
        }

        // GET: Home/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Home/Create
        [HttpPost]
        public ActionResult Create(FakturaTable newFaktura)
        {

            if (!ModelState.IsValid)
                return View();

            int faktureID = 0;
            int faktureIDmax = 0;
            string sql = "";

            //broj redova u tabeli Faktura
            string sqlCount = @"SELECT COUNT(*) FROM FakturaTable";
            int count = _db.Database.SqlQuery<int>(sqlCount).Single();


            if (count != 0)
            {
                sql = @"SELECT MAX(fakturaID) FROM FakturaTable"; 
                faktureIDmax = _db.Database.SqlQuery<int>(sql).Single();
                faktureID = faktureIDmax + 1;
            }

            _db.FakturaTables.Add(new FakturaTable
            {
                fakturaID = faktureID,
                datum_fakture = newFaktura.datum_fakture,
                broj_fakture = newFaktura.broj_fakture,
                ukupno = newFaktura.ukupno,
                datum_dokumenta = newFaktura.datum_dokumenta,
                broj_dokumenta = newFaktura.broj_dokumenta,
                ukupno_stavki = newFaktura.StavkaFaktureTables.Count
            });
            _db.SaveChanges();


            return RedirectToAction("Index");
          
        }

        // GET: Home/CreateStavka
        public ActionResult CreateStavka()
        {
            return View();
        }

        // POST: Home/CreateStavka/5001
        [HttpPost]
        public ActionResult CreateStavka(int ID, StavkaFaktureTable newStavkaFakture)
        {

            if (!ModelState.IsValid)
                return View();

            var ukupno = newStavkaFakture.cena * newStavkaFakture.kolicina;
            int redniBr = 0;
            int redniBrmax = 0;
            string sqlMax = "";
            
            //broj redova u tabeli Stavke
            string sqlCount = @"SELECT COUNT(*) FROM StavkaFaktureTable"; 
            int count = _db.Database.SqlQuery<int>(sqlCount).Single();

            if(count != 0)
            {
                sqlMax = @"SELECT MAX(redniBr) FROM StavkaFaktureTable";
                redniBrmax = _db.Database.SqlQuery<int>(sqlMax).Single();
                redniBr = redniBrmax + 1; //redni broj postavljano za 1 veci od max rednog broja u tabeli
            }

            _db.StavkaFaktureTables.Add( new StavkaFaktureTable
            {
                redniBr = redniBr,
                fakturaID = ID,
                kolicina = newStavkaFakture.kolicina,
                cena = newStavkaFakture.cena,
                ukupno = ukupno
            });

            _db.SaveChanges();

            //kada se doda nova stavka treba vrednost ukupno_stavki u tabeli Faktura da promenimo i da dodamo tu stavku u listuStavki
            int countSF = 0;
           
            string esqlQuery = "";
            int counter = 0;

            using (EntityConnection conn = new EntityConnection("name=FaktureDBEntities"))
            {
                conn.Open();

                esqlQuery = @"SELECT VALUE sf FROM FaktureDBEntities.StavkaFaktureTables as sf where sf.fakturaID = @id";

                using (EntityCommand cmd = new EntityCommand(esqlQuery, conn))
                {
                    //kreiramo parametar za parametarski upit
                    EntityParameter param1 = new EntityParameter();
                    param1.ParameterName = "id";
                    param1.Value = ID;

                    cmd.Parameters.Add(param1);

                    using (DbDataReader rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess))

                    {
                        while (rdr.Read())
                        {
                            //broj redova u tabeli Stavki za dati fakturaID
                            counter++;
                        }

                    }
                }
                
            }

            countSF = counter;

            var faktura = _db.FakturaTables.Find(ID);

            var result = _db.FakturaTables.SingleOrDefault(f => f.fakturaID == faktura.fakturaID);

            if (!ModelState.IsValid)

                return View(faktura);

            if (result != null)
            {
                result.ukupno_stavki = countSF;
                _db.SaveChanges();
            }

            faktura.StavkaFaktureTables.Add(newStavkaFakture);

            return RedirectToAction("Index");

        }

        // GET: Home/Edit/5
        public ActionResult Edit(int id)
        {
            var fakturaUpdate = (from f in _db.FakturaTables

                               where f.fakturaID == id

                               select f).First();

            return View(fakturaUpdate);
        }

        // POST: Home/Edit/5
        [HttpPost]
        public ActionResult Edit(FakturaTable fakturaEdit)
        {
            var result = _db.FakturaTables.SingleOrDefault(f => f.fakturaID == fakturaEdit.fakturaID);

            if (!ModelState.IsValid)

                return View(fakturaEdit);

            if (result != null)
            {
                result.datum_fakture = fakturaEdit.datum_fakture;
                result.broj_fakture = fakturaEdit.broj_fakture;
                result.ukupno = fakturaEdit.ukupno;
                result.datum_dokumenta = (DateTime)(fakturaEdit.datum_dokumenta);
                result.broj_dokumenta = fakturaEdit.broj_dokumenta;
                _db.SaveChanges();

            }

            return RedirectToAction("Index");
        }

        // GET: Home/EditStavka/5
        public ActionResult EditStavka(int id)
        {
            var stavkaUpdate = (from f in _db.StavkaFaktureTables

                                 where f.redniBr == id

                                 select f).First();

            return View(stavkaUpdate);
        }

        // POST: Home/EditStavka/5
        [HttpPost]
        public ActionResult EditStavka(StavkaFaktureTable stavkaEdit)
        {
            var result = _db.StavkaFaktureTables.SingleOrDefault(f => f.redniBr == stavkaEdit.redniBr);

            if (!ModelState.IsValid)

                return View(stavkaEdit);

            if (result != null)
            {
                result.kolicina = stavkaEdit.kolicina;
                result.cena = stavkaEdit.cena;
                result.ukupno = stavkaEdit.kolicina * stavkaEdit.cena;

                _db.SaveChanges();

            }

            return RedirectToAction("Index");
        }

        // GET: Home/Delete/5
        public ActionResult Delete(int id)
        {
            var fakturaDelete = (from f in _db.FakturaTables

                                 where f.fakturaID == id

                                 select f).First();

            return View(fakturaDelete);
        }


        // POST: Home/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var fakturaDelete = (from f in _db.FakturaTables

                                 where f.fakturaID == id

                                 select f).First();

            //obrisi sve stavke fakture
            var listaStavkiFakture = fakturaDelete.StavkaFaktureTables.ToList();
            listaStavkiFakture.ForEach(x => _db.StavkaFaktureTables.Remove(x));

            //obrisi fakturu
            _db.FakturaTables.Remove(_db.FakturaTables.Single(f => f.fakturaID == fakturaDelete.fakturaID));

            _db.SaveChanges();

            if (!ModelState.IsValid)

                return View(fakturaDelete);

            return RedirectToAction("Index");

        }

        // GET: Home/DeleteStavka/5
        public ActionResult DeleteStavka(int id)
        {
            var stavkaFaktureDelete = (from f in _db.StavkaFaktureTables

                                 where f.redniBr == id

                                 select f).First();

            return View(stavkaFaktureDelete);
        }

        // POST: Home/DeleteStavka/5
        [HttpPost, ActionName("DeleteStavka")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteStavkaConfirmed(int id)
        {

            var stavkaDelete = (from f in _db.StavkaFaktureTables

                                       where f.redniBr == id

                                       select f).First();

            var faktura = _db.FakturaTables.Find(stavkaDelete.fakturaID);

            var result = _db.FakturaTables.SingleOrDefault(f => f.fakturaID == faktura.fakturaID);
 
            if (result != null)
            {
                result.ukupno_stavki = faktura.StavkaFaktureTables.Count - 1;
                _db.SaveChanges();
            }

            faktura.StavkaFaktureTables.Remove(stavkaDelete);

            _db.StavkaFaktureTables.Remove(_db.StavkaFaktureTables.Single(f => f.redniBr == stavkaDelete.redniBr));
            _db.SaveChanges();

            if (!ModelState.IsValid)

                return View(stavkaDelete);

            return RedirectToAction("Index");
          
        }
    }
}
