using System;
using System.Collections.Generic;
using MySqlConnector;

namespace JuegoAsteroides
{
    public class NaveCatalogoItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public float VelocidadBase { get; set; }
        public int VidasMaximas { get; set; }
        public float CadenciaDisparo { get; set; }
        public int PrecioMonedas { get; set; }
        public bool Desbloqueada { get; set; }
        public bool Equipada { get; set; }
    }

    public class DatabaseManager
    {
        private readonly string _connectionString = "Server=localhost;Port=3307;Database=asteroid_destroy;Uid=root;Pwd=;";

        public DatabaseManager(string server = "127.0.0.1", int port = 3307, string database = "asteroid_destroy", string user = "root", string password = "")
        {
            _connectionString = $"Server={server};Database={database};Uid={user};Pwd={password};CharSet=utf8mb4;";
        }

        public bool ProbarConexion()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] {ex.Message}");
                return false;
            }
        }

        public int ObtenerOCrearJugador(string userName)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string queryBuscar = "SELECT id FROM players WHERE user_name = @userName LIMIT 1;";
                using (var cmdBuscar = new MySqlCommand(queryBuscar, conn))
                {
                    cmdBuscar.Parameters.AddWithValue("@userName", userName);
                    object resultado = cmdBuscar.ExecuteScalar();
                    if (resultado != null)
                        return Convert.ToInt32(resultado);
                }

                string queryInsertar = @"
                    INSERT INTO players (user_name, hp, xp, level, monedas, llaves_gacha)
                    VALUES (@userName, 100, 0, 1, 500, 3);
                    SELECT LAST_INSERT_ID();";

                using (var cmdInsertar = new MySqlCommand(queryInsertar, conn))
                {
                    cmdInsertar.Parameters.AddWithValue("@userName", userName);
                    int nuevoId = Convert.ToInt32(cmdInsertar.ExecuteScalar());

                    string queryNaveInicial = "INSERT IGNORE INTO player_ships (player_id, ship_id, equipada) VALUES (@playerId, 1, TRUE);";
                    using var cmdNave = new MySqlCommand(queryNaveInicial, conn);
                    cmdNave.Parameters.AddWithValue("@playerId", nuevoId);
                    cmdNave.ExecuteNonQuery();

                    return nuevoId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] {ex.Message}");
                return -1;
            }
        }

        public int IniciarSesion(int playerId, float dificultadInicial = 1.0f)
        {
            if (playerId <= 0) return -1;
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string query = @"
                    INSERT INTO sessions (player_id, inicio, dificultad_inicial)
                    VALUES (@playerId, NOW(), @dificultad);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@playerId", playerId);
                cmd.Parameters.AddWithValue("@dificultad", dificultadInicial);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] {ex.Message}");
                System.Diagnostics.Debug.WriteLine("ERROR MYSQL: " + ex.Message);
                return -1;
            }

        }

        public void FinalizarSesion(int sessionId, int tiempoSupervivenciaSegundos)
        {
            if (sessionId <= 0) return;

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string query = @"
                    UPDATE sessions
                    SET fin = NOW(), tiempo_supervivencia = @tiempo
                    WHERE id = @sessionId;";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@tiempo", tiempoSupervivenciaSegundos);
                cmd.Parameters.AddWithValue("@sessionId", sessionId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] {ex.Message}");
            }
        }

        public void RegistrarColision(int sessionId, int asteroidTypeId, string coordenada, int danoRecibido)
        {
            if (sessionId <= 0) return;

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string query = @"
                    INSERT INTO collisionlogs (session_id, asteroid_id, timestamp, coordenada_impacto, dano_recibido)
                    VALUES (@sessionId, @asteroidId, NOW(), @coord, @dano);";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sessionId", sessionId);
                cmd.Parameters.AddWithValue("@asteroidId", asteroidTypeId);
                cmd.Parameters.AddWithValue("@coord", coordenada);
                cmd.Parameters.AddWithValue("@dano", danoRecibido);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] {ex.Message}");
            }
        }

        public void RegistrarEvasion(int sessionId, int asteroidTypeId, decimal distanciaMinima)
        {
            if (sessionId <= 0) return;

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string query = @"
                    INSERT INTO dodgerecords (session_id, asteroid_id, timestamp, distancia_minima)
                    VALUES (@sessionId, @asteroidId, NOW(), @distancia);";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sessionId", sessionId);
                cmd.Parameters.AddWithValue("@asteroidId", asteroidTypeId);
                cmd.Parameters.AddWithValue("@distancia", distanciaMinima);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] {ex.Message}");
            }
        }

        public void RegistrarPowerUp(int sessionId, int powerUpId, int duracionSegundos)
        {
            if (sessionId <= 0) return;

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string query = @"
                    INSERT INTO poweruplogs (session_id, powerup_id, timestamp_aplicacion, timestamp_fin)
                    VALUES (@sessionId, @powerUpId, NOW(), DATE_ADD(NOW(), INTERVAL @duracion SECOND));";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sessionId", sessionId);
                cmd.Parameters.AddWithValue("@powerUpId", powerUpId);
                cmd.Parameters.AddWithValue("@duracion", duracionSegundos);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] {ex.Message}");
            }
        }

        public List<NaveCatalogoItem> ObtenerNavesDeJugador(int playerId)
        {
            var lista = new List<NaveCatalogoItem>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string query = @"
                    SELECT s.id, s.nombre, s.velocidad_base, s.vidas_maximas, s.cadencia_disparo, s.precio_monedas,
                           ps.ship_id IS NOT NULL AS desbloqueada,
                           COALESCE(ps.equipada, FALSE) AS equipada
                    FROM ships s
                    LEFT JOIN player_ships ps ON s.id = ps.ship_id AND ps.player_id = @playerId
                    ORDER BY s.precio_monedas ASC;";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@playerId", playerId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new NaveCatalogoItem
                    {
                        Id = reader.GetInt32("id"),
                        Nombre = reader.GetString("nombre"),
                        VelocidadBase = reader.GetFloat("velocidad_base"),
                        VidasMaximas = reader.GetInt32("vidas_maximas"),
                        CadenciaDisparo = reader.GetFloat("cadencia_disparo"),
                        PrecioMonedas = reader.GetInt32("precio_monedas"),
                        Desbloqueada = reader.GetBoolean("desbloqueada"),
                        Equipada = reader.GetBoolean("equipada")
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] {ex.Message}");
            }

            return lista;
        }

        public bool ComprarNave(int playerId, int shipId)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();
                using var transaction = conn.BeginTransaction();

                int monedasActuales = 0;
                int precioNave = 0;

                using (var cmdMonedas = new MySqlCommand("SELECT monedas FROM players WHERE id = @playerId FOR UPDATE;", conn, transaction))
                {
                    cmdMonedas.Parameters.AddWithValue("@playerId", playerId);
                    object res = cmdMonedas.ExecuteScalar();
                    if (res == null) return false;
                    monedasActuales = Convert.ToInt32(res);
                }

                using (var cmdPrecio = new MySqlCommand("SELECT precio_monedas FROM ships WHERE id = @shipId;", conn, transaction))
                {
                    cmdPrecio.Parameters.AddWithValue("@shipId", shipId);
                    object res = cmdPrecio.ExecuteScalar();
                    if (res == null) return false;
                    precioNave = Convert.ToInt32(res);
                }

                if (monedasActuales < precioNave)
                {
                    transaction.Rollback();
                    return false;
                }

                using (var cmdDescontar = new MySqlCommand("UPDATE players SET monedas = monedas - @precio WHERE id = @playerId;", conn, transaction))
                {
                    cmdDescontar.Parameters.AddWithValue("@precio", precioNave);
                    cmdDescontar.Parameters.AddWithValue("@playerId", playerId);
                    cmdDescontar.ExecuteNonQuery();
                }

                using (var cmdAsignar = new MySqlCommand("INSERT INTO player_ships (player_id, ship_id, equipada) VALUES (@playerId, @shipId, FALSE);", conn, transaction))
                {
                    cmdAsignar.Parameters.AddWithValue("@playerId", playerId);
                    cmdAsignar.Parameters.AddWithValue("@shipId", shipId);
                    cmdAsignar.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] {ex.Message}");
                return false;
            }
        }
    }
}
