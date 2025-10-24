using System;
using System.Collections.Generic;
using System.Linq;
using CMCSPrototype.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CMCSPrototype.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly IWebHostEnvironment _env;

        // For demonstration, inject environment (mockable in tests)
        public ClaimsController(IWebHostEnvironment env = null)
        {
            _env = env;
        }

        // --- In-memory storage ---
        private static readonly List<Claim> SampleClaims = new()
        {
            new Claim { Id = 1, LecturerName = "t.mokoena", ClaimMonth = "March 2025", HoursWorked = 12, HourlyRate = 450, Status = "Pending", SupportingDocument = "timesheet_mar.pdf" },
            new Claim { Id = 2, LecturerName = "n.khanye",   ClaimMonth = "March 2025", HoursWorked = 8,  HourlyRate = 500, Status = "Approved", SupportingDocument = "timesheet_mar.pdf" },
            new Claim { Id = 3, LecturerName = "a.naidoo",   ClaimMonth = "April 2025", HoursWorked = 10, HourlyRate = 420, Status = "Pending", SupportingDocument = "evidence.zip" }
        };

        // -------------------------------
        // GET: Claims/Submit
        // -------------------------------
        [HttpGet]
        public IActionResult Submit()
        {
            return View(new Claim());
        }

        // -------------------------------
        // POST: Claims/Submit
        // -------------------------------
        [HttpPost]
        public IActionResult Submit(Claim claim)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "⚠️ Please correct the highlighted errors.";
                    return View(claim);
                }

                // Handle file upload
                if (claim.UploadedFile != null && claim.UploadedFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{claim.UploadedFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        claim.UploadedFile.CopyTo(fileStream);
                    }

                    claim.SupportingDocument = uniqueFileName;
                }

                // Assign new Id and default status
                claim.Id = SampleClaims.Max(c => c.Id) + 1;
                claim.Status = "Pending";

                SampleClaims.Add(claim);

                TempData["PrototypeMessage"] = "✅ Claim submitted successfully!";
                return RedirectToAction(nameof(Track));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ An unexpected error occurred: {ex.Message}";
                return View(claim);
            }
        }


        // -------------------------------
        // GET: Claims/Verify
        // -------------------------------
        public IActionResult Verify()
        {
            try
            {
                return View(SampleClaims.ToList());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Could not load claims: {ex.Message}";
                return View(new List<Claim>());
            }
        }

        // -------------------------------
        // POST: Claims/Approve
        // -------------------------------
        [HttpPost]
        public IActionResult Approve(int id)
        {
            try
            {
                var claim = SampleClaims.FirstOrDefault(c => c.Id == id);
                if (claim == null)
                {
                    TempData["PrototypeMessage"] = $"⚠️ Claim #{id} not found.";
                    return RedirectToAction(nameof(Verify));
                }

                claim.Status = "Approved";
                TempData["PrototypeMessage"] = $"✅ Claim #{id} approved successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error approving claim: {ex.Message}";
            }

            return RedirectToAction(nameof(Verify));
        }

        // -------------------------------
        // POST: Claims/Reject
        // -------------------------------
        [HttpPost]
        public IActionResult Reject(int id)
        {
            try
            {
                var claim = SampleClaims.FirstOrDefault(c => c.Id == id);
                if (claim == null)
                {
                    TempData["PrototypeMessage"] = $"⚠️ Claim #{id} not found.";
                    return RedirectToAction(nameof(Verify));
                }

                claim.Status = "Rejected";
                TempData["PrototypeMessage"] = $"❌ Claim #{id} rejected.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error rejecting claim: {ex.Message}";
            }

            return RedirectToAction(nameof(Verify));
        }

        // -------------------------------
        // GET: Claims/Track
        // -------------------------------
        public IActionResult Track()
        {
            try
            {
                return View(SampleClaims.ToList());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Could not retrieve claim list: {ex.Message}";
                return View(new List<Claim>());
            }
        }

        // -------------------------------
        // GET: Download
        // -------------------------------
        public IActionResult Download(int id)
        {
            var claim = SampleClaims.FirstOrDefault(c => c.Id == id);
            if (claim == null || string.IsNullOrEmpty(claim.SupportingDocument))
            {
                TempData["ErrorMessage"] = $"⚠️ Document not found for Claim #{id}.";
                return RedirectToAction(nameof(Track));
            }

            // Assuming files are stored in wwwroot/uploads
            var filePath = Path.Combine(_env.WebRootPath, "uploads", claim.SupportingDocument);
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = $"⚠️ File not found: {claim.SupportingDocument}";
                return RedirectToAction(nameof(Track));
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(filePath, contentType, claim.SupportingDocument);
        }
    }
}
