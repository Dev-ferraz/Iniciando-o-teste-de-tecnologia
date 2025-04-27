using Microsoft.AspNetCore.Mvc;
using apiToDo.DTO;
using apiToDo.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace apiToDo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarefasController : ControllerBase
    {
        private readonly Tarefas _tarefas;
        private readonly ILogger<TarefasController> _logger;

        public TarefasController(Tarefas tarefas, ILogger<TarefasController> logger)
        {
            _tarefas = tarefas;
            _logger = logger;
        }

        // Retorna a lista de tarefas
        [HttpGet("lstTarefas")]
        public ActionResult<List<TarefaDTO>> lstTarefas()
        {
            try
            {
                var tarefasList = _tarefas.lstTarefas();
                _logger.LogInformation("Lista de tarefas retornada com sucesso.");
                return Ok(tarefasList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao listar tarefas: {ex.Message}");
                return StatusCode(500, new { Message = "Erro ao listar tarefas: " + ex.Message });
            }
        }





        // Insere múltiplas tarefas
        [HttpPost("InserirTarefas")]
        public async Task<IActionResult> InserirTarefas([FromBody] List<TarefaDTO> tarefas)
        {
            try
            {
                // Inserir as tarefas
                await _tarefas.InserirTarefasAsync(tarefas);

                // Obter a lista atualizada de tarefas
                var tarefasAtualizadas = _tarefas.lstTarefas();

                // Retornar a lista atualizada de tarefas
                return Ok(tarefasAtualizadas);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
            }
        }






        // Insere uma única tarefa
        [HttpPost("Inserir")]
        public async Task<ActionResult<TarefaDTO>> InserirTarefa([FromBody] TarefaDTO tarefa)
        {
            try
            {
                if (tarefa == null)
                    return BadRequest(new { Message = "A tarefa não pode ser nula." });

                await _tarefas.InserirOuAtualizarTarefaAsync(tarefa);
                _logger.LogInformation($"Tarefa '{tarefa.DS_TAREFA}' inserida/atualizada com sucesso.");

                return Ok(new { Message = $"Tarefa '{tarefa.DS_TAREFA}' inserida com sucesso." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Erro de validação: {ex.Message}");
                return BadRequest(new { Message = $"Erro de validação: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao inserir tarefa: {ex.Message}");
                return StatusCode(500, new { Message = "Erro ao inserir tarefa: " + ex.Message });
            }
        }

        // Deleta uma tarefa com base no ID
        [HttpDelete("DeleteTask/{id}")]
        public async Task<ActionResult<List<TarefaDTO>>> DeletarTarefa(int id)
        {
            try
            {
                await _tarefas.DeletarTarefaAsync(id);
                _logger.LogInformation($"Tarefa com ID {id} deletada com sucesso.");
                var tarefasList = _tarefas.lstTarefas();
                return Ok(tarefasList);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Tarefa não encontrada: {ex.Message}");
                return NotFound(new { Message = $"Tarefa com ID {id} não encontrada." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao deletar tarefa: {ex.Message}");
                return StatusCode(500, new { Message = "Erro ao deletar tarefa: " + ex.Message });
            }


        }


        [HttpGet("VerificarDuplicidade")]
        public ActionResult VerificarDuplicidade([FromQuery] string descricao)
        {
            try
            {
                // Verifica se a tarefa com a descrição fornecida já existe
                var tarefaExistente = _tarefas.lstTarefas().FirstOrDefault(t => t.DS_TAREFA.Equals(descricao, StringComparison.OrdinalIgnoreCase));

                if (tarefaExistente != null)
                {
                    _logger.LogInformation($"A tarefa '{descricao}' já está cadastrada.");
                    return Ok(new { duplicado = true, mensagem = $"A tarefa '{descricao}' já está cadastrada." });
                }

                _logger.LogInformation($"A tarefa '{descricao}' não foi encontrada.");
                return Ok(new { duplicado = false, mensagem = $"A tarefa '{descricao}' não foi encontrada." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao verificar duplicidade: {ex.Message}");
                return StatusCode(500, new { Message = "Erro ao verificar duplicidade: " + ex.Message });
            }


        }

        // Atualiza uma tarefa existente
        [HttpPut("Atualizar/{id}")]
        public async Task<ActionResult<List<TarefaDTO>>> AtualizarTarefa(int id, [FromBody] TarefaDTO tarefaAtualizada)
        {
            try
            {
                // Verifica se a tarefa recebida é válida
                if (tarefaAtualizada == null)
                    return BadRequest(new { Message = "A tarefa não pode ser nula." });

                // Verifica se a tarefa com o ID fornecido existe
                var tarefaExistente = _tarefas.lstTarefas().FirstOrDefault(t => t.ID_TAREFA == id);

                // Se a tarefa não existir, retorna NotFound
                if (tarefaExistente == null)
                    return NotFound(new { Message = $"Tarefa com ID {id} não encontrada." });

                // Define o ID da tarefa atualizada para garantir que está atualizando a tarefa correta
                tarefaAtualizada.ID_TAREFA = id;

                // Atualiza ou insere a tarefa no banco de dados
                await _tarefas.InserirOuAtualizarTarefaAsync(tarefaAtualizada);

                // Retorna a lista de tarefas atualizada
                var tarefasList = _tarefas.lstTarefas();

                // Log de sucesso
                _logger.LogInformation($"Tarefa com ID {id} atualizada com sucesso.");

                // Retorna a lista de tarefas atualizada
                return Ok(tarefasList);
            }
            catch (ArgumentException ex)
            {
                // Em caso de erro de validação
                _logger.LogWarning($"Erro de validação: {ex.Message}");
                return BadRequest(new { Message = $"Erro de validação: {ex.Message}" });
            }
            catch (Exception ex)
            {
                // Em caso de erro inesperado
                _logger.LogError($"Erro ao atualizar tarefa: {ex.Message}");
                return StatusCode(500, new { Message = "Erro ao atualizar tarefa: " + ex.Message });
            }
        }







    }

}
