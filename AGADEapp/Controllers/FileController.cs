﻿using AGADEapp.Data.Configration;
using AGADEapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using AGADEapp.Services;
using System.ComponentModel;
using AGADEapp.Models.InputModels;

namespace AGADEapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IUserService _userService;
        private readonly FileDBContext _fileDBcontext;

        private static IWebHostEnvironment _webHostEnvironment;
        public FileController(FileDBContext dBContext, IFileService fileService, IUserService userService, IWebHostEnvironment webHostEnvironment)
        {
            _fileService = fileService;
            _userService = userService;
            _fileDBcontext = dBContext;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [DisplayName("Get All")]
        public async Task<IEnumerable<DataFile>> GetAll()
        {
            return await _fileService.GetAllFiles();
        }

        [HttpGet("{id}")]
        [DisplayName("Get File by ID")]
        [ProducesResponseType(typeof(DataFile), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFileById(int id)
        {
            var file = await _fileService.GetFileById(id);
            return file == null ? NotFound() : Ok(file);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromForm]DataFileInit file)
        {
            var created = await _fileService.CreateFile(DataFile.of(file));
            return created == null ? NotFound() : Ok(created);
        }

        [HttpPost]
        [Route("upload")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Upload([FromForm] UploadFile obj)
        {
            if (obj.file.Length > 0)
            {
                try
                {
                    if (!Directory.Exists(_webHostEnvironment.WebRootPath + "\\Files\\"))
                    {
                        Directory.CreateDirectory(_webHostEnvironment.WebRootPath + "\\Files\\");
                    }

                    using (FileStream fileStream = System.IO.File.Create(_webHostEnvironment.WebRootPath + "\\Files\\" + obj.file.FileName))
                    {
                        obj.file.CopyTo(fileStream);
                        fileStream.Flush();

                        var edit = await _fileService.GetFileById(obj.dataFileId);
                        edit.Content = obj.file.FileName;
                        edit.ContentType = obj.file.ContentType;
                        await _fileService.UpdateFile(obj.dataFileId, edit);

                        return Ok();
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else { return BadRequest(); }
        }


        [HttpGet("{id}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _fileService.GetFileById(id);

            if (file == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Files", file.Content);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            memoryStream.Position = 0;

            return File(memoryStream, file.ContentType, file.Content);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateFile(int id, DataFile file)
        {
            //var _file = await _fileDBcontext.DataFile.FindAsync(id);

            //if (file == null)
            //{
            //    return BadRequest();
            //}

            //_fileDBcontext.Entry(file).State = EntityState.Modified;
            //await _fileDBcontext.SaveChangesAsync();

            //return NoContent();

            var _file = await _fileService.UpdateFile(id, file);

            return _file == null ? NotFound() : Ok(file);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _fileService.DeleteFile(id);
            } catch 
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}
