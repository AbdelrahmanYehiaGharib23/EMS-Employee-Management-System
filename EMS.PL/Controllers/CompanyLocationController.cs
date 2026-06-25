using EMS.DAL.Contracts.UnitOfWork;
using EMS.DAL.Entities.Location;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.PL.Controllers
{
   

   
        [Authorize(Roles = "Admin")]
        public class CompanyLocationController : Controller
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IConfiguration _configuration;

            public CompanyLocationController(IUnitOfWork unitOfWork, IConfiguration configuration)
            {
                _unitOfWork = unitOfWork;
                _configuration = configuration;
            }

            public async System.Threading.Tasks.Task<IActionResult> Index()
            {
                var location = (await _unitOfWork.CompanyLocationRepository.GetAllAsync())
                    .FirstOrDefault();

                if (location == null)
                {
                    location = new CompanyLocation();
                }

                ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
                return View(location);
            }

            [HttpGet]
            public async System.Threading.Tasks.Task<IActionResult> GetLocation()
            {
                var location = (await _unitOfWork.CompanyLocationRepository.GetAllAsync()).FirstOrDefault();
                if (location == null) return Json(null);
                return Json(new
                {
                    name = location.Name,
                    latitude = location.Latitude,
                    longitude = location.Longitude,
                    radius = location.AllowedRadiusMeters
                });
            }

            [HttpPost]
            public async System.Threading.Tasks.Task<IActionResult> Save(CompanyLocation model)
            {
                if (!ModelState.IsValid)
                    return View("Index", model);

                var existing = (await _unitOfWork.CompanyLocationRepository.GetAllAsync())
                    .FirstOrDefault();

                if (existing == null)
                {
                    _unitOfWork.CompanyLocationRepository.Add(model);
                }
                else
                {
                    existing.Name = model.Name;
                    existing.Latitude = model.Latitude;
                    existing.Longitude = model.Longitude;
                    existing.AllowedRadiusMeters = model.AllowedRadiusMeters;

                    _unitOfWork.CompanyLocationRepository.Update(existing);
                }

                await _unitOfWork.CompleteAsync();

                TempData["Success"] = "Company location saved successfully";

                return RedirectToAction(nameof(Index));
            }
        }
    }
