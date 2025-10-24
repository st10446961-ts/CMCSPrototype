using Xunit;
using Moq;
using CMCSPrototype.Controllers;
using CMCSPrototype.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

namespace CMCSPrototype.Tests
{
    public class ClaimsControllerReliabilityTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly ClaimsController _controller;

        public ClaimsControllerReliabilityTests()
        {
            // Mock IWebHostEnvironment
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.EnvironmentName).Returns("Development");
            _mockEnv.Setup(e => e.WebRootPath).Returns("wwwroot");

            // Create controller
            _controller = new ClaimsController(_mockEnv.Object);

            // Initialize TempData to prevent NullReferenceException
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );
        }

        // -------------------------
        // GET Submit()
        // -------------------------
        [Fact]
        public void Submit_Get_ReturnsViewWithEmptyModel()
        {
            var result = _controller.Submit() as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<Claim>(result.Model);
        }

        // -------------------------
        // POST Submit (Valid)
        // -------------------------
        [Fact]
        public void Submit_ValidClaim_RedirectsToTrack()
        {
            var claim = new Claim
            {
                LecturerName = "J.Longmore",
                ClaimMonth = "October 2025",
                HoursWorked = 10,
                HourlyRate = 100
            };

            var result = _controller.Submit(claim) as RedirectToActionResult;

            Assert.Equal("Track", result.ActionName);

            var allClaims = (_controller.Track() as ViewResult).Model as List<Claim>;
            Assert.Contains(allClaims, c => c.LecturerName == "J.Longmore");
            Assert.Equal("✅ Claim submitted successfully!", _controller.TempData["PrototypeMessage"]);
        }

        // -------------------------
        // POST Submit (Invalid)
        // -------------------------
        [Fact]
        public void Submit_InvalidClaim_ReturnsViewWithErrors()
        {
            var invalidClaim = new Claim(); // Missing required fields
            _controller.ModelState.AddModelError("LecturerName", "Required");

            var result = _controller.Submit(invalidClaim) as ViewResult;

            Assert.Equal(invalidClaim, result.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        // -------------------------
        // GET Verify()
        // -------------------------
        [Fact]
        public void Verify_ReturnsListOfClaims()
        {
            var result = _controller.Verify() as ViewResult;

            Assert.NotNull(result);
            var claims = Assert.IsAssignableFrom<List<Claim>>(result.Model);
            Assert.True(claims.Count >= 3); // Includes initial sample claims
        }

        // -------------------------
        // Approve Valid ID
        // -------------------------
        [Fact]
        public void Approve_ValidId_UpdatesStatusToApproved()
        {
            var claim = new Claim { LecturerName = "Alice", ClaimMonth = "Nov 2025", HoursWorked = 5, HourlyRate = 150 };
            _controller.Submit(claim);

            var allClaims = (_controller.Track() as ViewResult).Model as List<Claim>;
            var lastClaim = allClaims.Last();

            var result = _controller.Approve(lastClaim.Id) as RedirectToActionResult;

            Assert.Equal("Verify", result.ActionName);
            Assert.Equal("Approved", lastClaim.Status);
        }

        // -------------------------
        // Approve Invalid ID
        // -------------------------
        [Fact]
        public void Approve_InvalidId_SetsWarningMessage()
        {
            var result = _controller.Approve(999) as RedirectToActionResult;

            Assert.Equal("Verify", result.ActionName);
            Assert.Equal("⚠️ Claim #999 not found.", _controller.TempData["PrototypeMessage"]);
        }

        // -------------------------
        // Reject Valid ID
        // -------------------------
        [Fact]
        public void Reject_ValidId_UpdatesStatusToRejected()
        {
            var claim = new Claim { LecturerName = "Bob", ClaimMonth = "Dec 2025", HoursWorked = 8, HourlyRate = 120 };
            _controller.Submit(claim);

            var allClaims = (_controller.Track() as ViewResult).Model as List<Claim>;
            var lastClaim = allClaims.Last();

            var result = _controller.Reject(lastClaim.Id) as RedirectToActionResult;

            Assert.Equal("Verify", result.ActionName);
            Assert.Equal("Rejected", lastClaim.Status);
        }

        // -------------------------
        // Reject Invalid ID
        // -------------------------
        [Fact]
        public void Reject_InvalidId_SetsWarningMessage()
        {
            var result = _controller.Reject(999) as RedirectToActionResult;

            Assert.Equal("Verify", result.ActionName);
            Assert.Equal("⚠️ Claim #999 not found.", _controller.TempData["PrototypeMessage"]);
        }

        // -------------------------
        // Track()
        // -------------------------
        [Fact]
        public void Track_ReturnsAllClaims()
        {
            var result = _controller.Track() as ViewResult;

            Assert.NotNull(result);
            var claims = Assert.IsAssignableFrom<List<Claim>>(result.Model);
            Assert.NotEmpty(claims);
        }

        // -------------------------
        // Environment Name Test
        // -------------------------
        [Fact]
        public void Environment_IsMocked_Successfully()
        {
            // Only test EnvironmentName; no need to verify WebRootPath
            Assert.Equal("Development", _mockEnv.Object.EnvironmentName);
        }
    }
}
