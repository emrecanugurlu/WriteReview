using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Dtos.ResponseDto;
using WriteReview.Persistence.Contexts;
using WriteReview.Persistence.Security;

namespace WriteReview.Persistence.Services.ExpertiseArea
{
    public class ExpertiseAreaService
    {


        public List<ExpertiseAreaWithoutUsers> GetAllExpertiseAreaWithoutUsers(WriteReviewDbContext db)
        {
           var expertiseArea =  db.ExpertiseAreas.Select(x=> new ExpertiseAreaWithoutUsers
           {
               Id = x.Id,
               Name = x.Name,
           }).ToList();
            
            return expertiseArea;
        }

        public  List<ExpertiseAreaWithUsers> GetAllExpertiseAreaWithUsers(WriteReviewDbContext db)
        {
            var expertiseArea = db.ExpertiseAreas.Select(e => new ExpertiseAreaWithUsers
            {
                Id = e.Id,
                Name = e.Name,
                Users = e.Users.Select(ue => new ExpertiseAreaUser { Id = ue.UserId, Name= ue.User.FullName}).ToList()
            }).ToList();
            

            return expertiseArea;
        }

        /// <summary>
        /// Veritabanına uzmanlık alanı eklemek için kullanılar fonksiyon.
        /// </summary>
        /// <param name="db"></param>
        /// 
        /// <param name="dto"></param>
        /// <returns>
        /// Geri dönüş tipi olarak metinsel (string) ifadeler dönmektedir.
        /// Eğer işlem başarısız ise "Ekleme işlemi sırasında hata oluştu", başarılı ise "Ekleme işlemi başarılı" döner.
        /// </returns> 
        public string AddExpertiseArea(WriteReviewDbContext db, AddExpertiseAreaDto dto)
        {
            try
            {
                var entityEntry = db.ExpertiseAreas.Add(new() { Id = new Guid(), Name = dto.Name });
                db.SaveChanges();

                return "Ekleme işlemi başarılı";
            }
            catch
            {
                return "Ekleme işlemi sırasında hata oluştu";
            }

        }


    }
}
