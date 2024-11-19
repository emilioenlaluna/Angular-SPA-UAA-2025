namespace API.Controllers;

using System.Security.Claims;
using API.Data;
using API.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public UsersController(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtiene una lista de todos los miembros.
    /// </summary>
    /// <returns>Una lista de objetos MemberResponse.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberResponse>>> GetAllAsync()
    {
        try
        {
            var members = await _repository.GetMembersAsync();
            return Ok(members);
        }
        catch (Exception ex)
        {
            // Aquí puedes registrar la excepción si tienes un servicio de logging
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Obtiene la información de un miembro específico por su nombre de usuario.
    /// </summary>
    /// <param name="username">El nombre de usuario del miembro.</param>
    /// <returns>Un objeto MemberResponse si se encuentra, de lo contrario NotFound.</returns>
    [HttpGet("{username}")] // Ejemplo de ruta: api/users/Calamardo
    public async Task<ActionResult<MemberResponse>> GetByUsernameAsync(string username)
    {
        try
        {
            var member = await _repository.GetMemberAsync(username);

            if (member == null)
            {
                return NotFound();
            }

            return Ok(member);
        }
        catch (Exception ex)
        {
            // Aquí puedes registrar la excepción si tienes un servicio de logging
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Actualiza la información del usuario autenticado.
    /// </summary>
    /// <param name="request">Los datos de actualización del miembro.</param>
    /// <returns>NoContent si la actualización es exitosa, de lo contrario BadRequest.</returns>
    [HttpPut]
    public async Task<ActionResult> UpdateUser([FromBody] MemberUpdateRequest request)
    {
        // Verificar si el modelo es inválido
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("No username found in token");
        }

        var user = await _repository.GetByUsernameAsync(username);

        if (user == null)
        {
            return BadRequest("Could not find user");
        }

        _mapper.Map(request, user);
        _repository.Update(user);

        if (await _repository.SaveAllAsync())
        {
            return NoContent();
        }

        return BadRequest("Update user failed!");
    }
}
