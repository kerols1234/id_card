using idCard.Data;
using idCard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace idCard.Controllers
{
    [Route("api/[controller]/[action]")]
    [EnableCors]
    [ApiController]
    public class IdCardController : ControllerBase
    {
        private readonly AppDbContext _db;
        public readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public IdCardController(AppDbContext db, IHttpContextAccessor context, IWebHostEnvironment hostingEnvironment)
        {
            _httpContextAccessor = context;
            _hostingEnvironment = hostingEnvironment;
            _db = db;
        }

        [HttpGet]
        public IActionResult getAllIdCards()
        {
            var webRootPath = _httpContextAccessor.HttpContext.Request.Host.ToString() ?? string.Empty;

            return Ok(_db.IdCards.Include(obj => obj.Attachments).Select(obj => new
            {
                Address = obj.Address,
                NationalId = obj.NationalId,
                ExpiryDate = obj.ExpiryDate,
                FullName = obj.FullName,
                FirstName = obj.FirstName,
                BirthDate = obj.BirthDate,
                Sex = obj.Sex,
                Governorate = obj.Governorate,
                Attachments = obj.Attachments.Select(o =>
                    Path.Combine(Path.Combine(webRootPath, WebConstant.DocumentPath + obj.NationalId),o.Document)
                ).ToList(),
            }).ToList());
        }

        [HttpGet]
        public IActionResult getIdCardById(string nationalId)
        {
            if (!_db.IdCards.Any(obj => obj.NationalId == nationalId))
                return BadRequest("No IdCard whith this NationalId");

            var webRootPath = _httpContextAccessor.HttpContext.Request.Host.ToString() ?? string.Empty;

            return Ok(_db.IdCards.Include(obj => obj.Attachments).Select(obj => new
            {
                Address = obj.Address,
                NationalId = obj.NationalId,
                ExpiryDate = obj.ExpiryDate,
                FullName = obj.FullName,
                FirstName = obj.FirstName,
                BirthDate = obj.BirthDate,
                Sex = obj.Sex,
                Governorate = obj.Governorate,
                Attachments = obj.Attachments.Select(o =>
                    Path.Combine(Path.Combine(webRootPath, WebConstant.DocumentPath + obj.NationalId), o.Document)
                ).ToList(),
            }).FirstOrDefault(obj => obj.NationalId == nationalId));
        }

        [HttpPost]
        public IActionResult Add([FromBody] IdCard idCard)
        {
            if (_db.IdCards.Any(obj => obj.NationalId == idCard.NationalId))
                return BadRequest("This IdCard already exists");

            _db.IdCards.Add(idCard);
            _db.SaveChanges();

            return Ok("Operation is done");
        }

        [HttpPost]
        public IActionResult Edit([FromBody] IdCard idCard)
        {
            if (!_db.IdCards.Any(obj => obj.NationalId == idCard.NationalId))
                return BadRequest("No IdCard whith this NationalId");
            var card = _db.IdCards.Include(obj => obj.Attachments).FirstOrDefault(obj => obj.NationalId == idCard.NationalId);
            card.FullName = idCard.FullName;
            card.FirstName = idCard.FirstName;
            card.Address = idCard.Address;
            card.BirthDate = idCard.BirthDate;
            card.ExpiryDate = idCard.ExpiryDate;
            card.Governorate = idCard.Governorate;
            card.Sex = idCard.Sex;
            _db.IdCards.Update(card);
            _db.SaveChanges();

            return Ok("Operation is done");
        }

        [HttpPost]
        public async Task<IActionResult> AddFilesAsync([FromForm] Filesmodel model)
        {

            if (!_db.IdCards.Any(obj => obj.NationalId == model.NationalId))
                return BadRequest("No IdCard whith this NationalId");

            var card = _db.IdCards.Include(obj => obj.Attachments).FirstOrDefault(obj => obj.NationalId == model.NationalId);

            var webRootPath = _hostingEnvironment.WebRootPath ?? string.Empty;

            var fullPath = Path.Combine(webRootPath, WebConstant.DocumentPath + model.NationalId);

            model.files = model.files ?? new List<IFormFile>();

            var listOfPaths = await UploadFile(model.files, fullPath);

            card.Attachments = card.Attachments.Concat(listOfPaths.Select(obj => new Attachment { Document = obj }).ToHashSet()).ToHashSet();

            _db.IdCards.Update(card);
            _db.SaveChanges();

            return Ok("Operation is done");
        }

        [HttpPost]
        public async Task<IActionResult> EditFilesAsync([FromForm] Filesmodel model)
        {
            if (!_db.IdCards.Any(obj => obj.NationalId == model.NationalId))
                return BadRequest("No IdCard whith this NationalId");

            var card = _db.IdCards.Include(obj => obj.Attachments).FirstOrDefault(obj => obj.NationalId == model.NationalId);

            if (model.files != null || model.files.Count() != 0)
            {
                var webRootPath = _hostingEnvironment.WebRootPath ?? string.Empty;

                var fullPath = Path.Combine(webRootPath, WebConstant.DocumentPath + model.NationalId);

                var listOfPaths = await UploadFile(model.files, fullPath);

                foreach (var attachment in card.Attachments)
                {
                    var oldFile = Path.Combine(fullPath, attachment.Document);

                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }
                }

                card.Attachments = listOfPaths.Select(obj => new Attachment { Document = obj }).ToHashSet();

                _db.IdCards.Update(card);
                _db.SaveChanges();
            }

            return Ok("Operation is done");

        }

        [HttpDelete]
        public IActionResult Delete(string id)
        {
            var obj = _db.IdCards.Include(obj => obj.Attachments).FirstOrDefault(obj => obj.NationalId == id);

            if (obj == null)
            {
                return BadRequest("No IdCard with this id");
            }
            try
            {
                var webRootPath = _hostingEnvironment.WebRootPath ?? string.Empty;
                string upload = Path.Combine(webRootPath, WebConstant.DocumentPath + obj.NationalId);

                foreach (var attachment in obj.Attachments)
                {
                    var oldFile = Path.Combine(upload, attachment.Document);

                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }
                }

                var result = _db.IdCards.Remove(obj);
                _db.SaveChanges();
                return Ok("Operation is done");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private async Task<List<string>> UploadFile(List<IFormFile> files, string fullPath)
        {
            Directory.CreateDirectory(fullPath);
            List<string> listOfPaths = new List<string>();

            foreach (var file in files)
            {
                if (file.Length <= 0) return listOfPaths;
                var fileName = Guid.NewGuid().ToString();
                var extension = Path.GetExtension(file.FileName);
                var filePath = Path.Combine(fullPath, fileName + extension);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    listOfPaths.Add(fileName + extension);
                }
            }
            return listOfPaths;
        }
    }
}
