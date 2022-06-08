using System.Collections.Generic;

namespace Halero.Models.GameManagement;
public class GameUpdate{
    public Guid? ID { get; set; }
    public SnakeDirection? direction { get; set; }
    public List<int[]>? tail { get; set; }
    public List<int[]>? candies { get; set; }
    public int? score { get; set; }
    public bool gameOver { get; set; } = false;
} 