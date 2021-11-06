using GestionPersonas.DAL;
using GestionPersonas.Entidades;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GestionPersonas.BLL
{
    public class AportesBLL
    {
        public static bool Guardar(Aportes aporte)
        {
            if (!Existe(aporte.AporteId))//si no existe insertamos
                return Insertar(aporte);
            else
                return Modificar(aporte);
        }
        private static bool Insertar(Aportes aporte)
        {
            bool paso = false;
            Contexto contexto = new Contexto();

            try
            {
                //Agregar la entidad que se desea insertar al contexto

                foreach (var detalle in aporte.DetalleAporte)
                {
                    detalle.TiposAporte.Logrado += aporte.Monto;

                    contexto.Entry(detalle.TiposAporte).State = EntityState.Modified;
                }
                contexto.Aportes.Add(aporte);

                paso = contexto.SaveChanges() > 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException.Message);
            }
            finally
            {
                contexto.Dispose();
            }
            return paso;
        }
        private static bool Modificar(Aportes aporte)
        {
            bool paso = false;
            Contexto contexto = new Contexto();

            try
            {
                var aporteAnterior = contexto.Aportes
                    .Where(x => x.AporteId == aporte.AporteId)
                    .Include(x => x.DetalleAporte)
                    .AsNoTracking()
                    .SingleOrDefault();

                //busca la entidad en la base de datos y la elimina
                foreach (var detalle in aporteAnterior.DetalleAporte)
                {
                    detalle.TiposAporte.Logrado -= detalle.Valor;
                }

                contexto.Database.ExecuteSqlRaw($"Delete FROM AportesDetalle Where Id={aporte.AporteId}");

                foreach (var item in aporte.DetalleAporte)
                {

                    item.TiposAporte.Logrado += item.Valor;
                    contexto.Entry(item).State = EntityState.Added;
                }

                //marcar la entidad como modificada para que el contexto sepa como proceder
                contexto.Entry(aporte).State = EntityState.Modified;
                paso = contexto.SaveChanges() > 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                contexto.Dispose();
            }
            return paso;
        }
        public static bool Eliminar(int id)
        {
            bool paso = false;
            Contexto contexto = new Contexto();

            try
            {
                //buscar la entidad que se desea eliminar
                var aporte = AportesBLL.Buscar(id);

                if (aporte != null)
                {
                    //busca la entidad en la base de datos y la elimina
                    foreach (var detalle in aporte.DetalleAporte)
                    {
                        detalle.TiposAporte.Logrado -= detalle.Valor;
                    }

                    contexto.Aportes.Remove(aporte); //remover la entidad
                    paso = contexto.SaveChanges() > 0;
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                contexto.Dispose();
            }
            return paso;
        }
        public static Aportes Buscar(int id)
        {
            Aportes aporte = new Aportes();
            Contexto contexto = new Contexto();

            try
            {
                aporte = contexto.Aportes.Include(x => x.DetalleAporte)
                    .Where(x => x.AporteId == id)
                     .Include(x => x.DetalleAporte)
                    .SingleOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                contexto.Dispose();
            }
            return aporte;
        }
        public static List<Aportes> GetList(Expression<Func<Aportes, bool>> criterio)
        {
            List<Aportes> Lista = new List<Aportes>();
            Contexto contexto = new Contexto();

            try
            {
                //obtener la lista y filtrarla según el criterio recibido por parametro.
                Lista = contexto.Aportes.Where(criterio).ToList();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                contexto.Dispose();
            }
            return Lista;
        }
        public static bool Existe(int id)
        {
            Contexto contexto = new Contexto();
            bool encontrado = false;

            try
            {
                encontrado = contexto.Aportes.Any(e => e.AporteId == id);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                contexto.Dispose();
            }

            return encontrado;
        }
    }
}
