import { SnakeDirection } from "../models/snake-direction-model";
import { SnakeGlobalConfig } from "../models/snake-config-model";
import { IGamePlayer } from "./game-player-interface";
import { GameState } from "../models/game-state-model";


export class MainPlayer implements IGamePlayer{
    protected tail: Array<[number, number]> = [ [ 150, 150 ] ];
    protected candies: Array<[number, number]> = []

    protected direction: SnakeDirection = SnakeDirection.Right;
    protected extend: number = 0;
    protected gameUpdate: NodeJS.Timer | null = null;

    public opponent: IGamePlayer | undefined = undefined;
    public snakeColor: string = "#C80000";
    public score: number = 0;
    public config: SnakeGlobalConfig;
    public gameCanvas: CanvasRenderingContext2D;
    public sessionID: string | undefined;
    public socket: WebSocket | undefined;
    public gameState: GameState = GameState.NotStarted;

    public onScoreChange: ((score: number) => void) | undefined = undefined;
    public onGameStateChange: ((gameState: GameState) => void) | undefined = undefined;

    constructor(    configuration: SnakeGlobalConfig, 
                    gameCanvas: CanvasRenderingContext2D,
                    tail: Array<[number, number]> | null = null, 
                    direction: SnakeDirection | null = null, 
                    socket: WebSocket | undefined = undefined,
                    sessionID: string | undefined = undefined,
                    isMainPlayer: boolean = true,
                ){
        if(tail !== null)
            this.tail = tail;
        if(direction !== null)
            this.direction = direction;
        this.config = configuration;
        this.gameCanvas = gameCanvas;
        this.socket = socket;
        console.log("created");

        if(isMainPlayer)
            document.getElementsByTagName("body")[0].addEventListener('keydown', e => this.changeDirection(e));
    }

    startGame(): void {
        
        if(this.onGameStateChange !== undefined && this.gameState !== GameState.NotStarted)
            this.onGameStateChange(GameState.Running);
        else if ( this.onGameStateChange !== undefined )
            this.onGameStateChange(GameState.NotStarted);
        this.gameState = GameState.Running;
        

        this.gameUpdate = setInterval( () => this.drawSnake(), this.config.frameRate );
    }

    pauseGame(): void {
        this.gameState = GameState.Waiting;
        
        if(this.onGameStateChange !== undefined)
            this.onGameStateChange(this.gameState);

        clearInterval(this.gameUpdate!);
    }

    toggleGame(){
        if(this.gameState === GameState.Waiting)
            this.startGame();
        else if(this.gameState === GameState.Running)
            this.pauseGame();
    }

    getTail(): { tail: Array<[number, number]> } { 
        return { tail: this.tail }; 
    }

    drawSnake() {

        let horizontal  = this.direction < 2 ? this.direction        * 2 - 1 : 0;
        let vertical    = this.direction >= 2 ? (this.direction % 2) * 2 - 1 : 0;      


        let x = this.tail[0][0] + (this.config.speed) * horizontal,
            y = this.tail[0][1] + (this.config.speed) * vertical;

        if( x > 300 )   x = 0;
        if( x < 0 )     x = 290;
        if( y > 300 )   y = 0;
        if( y < 0 )     y = 290;

        
        
        this.checkCandyIntercaption(x,y);
        if(this.candies.length === 0)
            this.getNewCandy();

        this.gameCanvas.fillStyle = this.snakeColor;
        this.gameCanvas.fillRect( x, y, this.config.size, this.config.size );

        if(this.extend === 0){
            this.gameCanvas.clearRect( this.tail[this.tail.length - 1][0], this.tail[this.tail.length - 1][1], this.config.size, this.config.size );
            this.tail.pop();     
        } 
        else{
            this.extend--;
        }

        
        this.tail.unshift([x, y]);
        // this.state.addance = [0,0];
        for(let i = 0; i < this.tail.length; i++){
            this.gameCanvas.fillRect( this.tail[i][0], this.tail[i][1], this.config.size, this.config.size );
        }
    }

    protected forceRedraw(){
    
            this.gameCanvas.fillStyle = "#FFFAEE";
            this.gameCanvas.fillRect( 0,0, this.config.width, this.config.height );
    
            this.gameCanvas.fillStyle = this.snakeColor;
            for(let i = 0; i < this.tail.length; i++){
                this.gameCanvas.fillRect( this.tail[i][0], this.tail[i][1], this.config.size, this.config.size );
            }
    
            if(this.opponent != undefined){
                this.gameCanvas.fillStyle = this.opponent!.snakeColor;
                for(let i = 0; i < this.opponent!.getTail().tail.length; i++){
                    this.gameCanvas.fillRect( this.opponent!.getTail().tail[i][0], this.opponent!.getTail().tail[i][1], this.config.size, this.config.size );
                }
            }
    
            let circleR = this.config.size / 3;
            this.gameCanvas.fillStyle = this.config.candyColor;
            for(let i = 0;i < this.candies.length; i++){
                this.gameCanvas.beginPath();
                this.gameCanvas.arc(this.candies[i][0] + circleR, this.candies[i][1] + circleR, circleR, 0, 2 * Math.PI, false);
                this.gameCanvas.fill();
            }
    }

    updateData(candies: Array<[number, number]>, opponent: IGamePlayer | undefined = undefined) {
        this.candies = candies;
        this.opponent = opponent
    }
    
    checkSnakeInterception(player: {tail: Array<[number, number]>}): boolean {
        throw "Not implrmrnted yet!!";
    }
    
    changeDirection(event: KeyboardEvent): void{
        let newDirection = this.direction;

        if(event.key === "p")
            this.toggleGame();
        if(event.key === "q"){
            if(this.sessionID === undefined)
                window.location.reload();
        }

        if(event.key === "ArrowLeft" && this.direction !== SnakeDirection.Left){
            newDirection = SnakeDirection.Right;
        }
        else if(event.key === "ArrowUp" && this.direction !== SnakeDirection.Down){
            newDirection = SnakeDirection.Up;
        }
        else if(event.key === "ArrowRight" && this.direction !== SnakeDirection.Right){
            newDirection = SnakeDirection.Left;
        }
        else if(event.key === "ArrowDown" && this.direction !== SnakeDirection.Up){
            newDirection = SnakeDirection.Down;
        }
        if(this.direction !== newDirection){
            this.direction = newDirection
            
            if(this.socket !== undefined)
                this.socket!.send( JSON.stringify({
                    ID: this.sessionID,
                    direction: newDirection,
                    tail: this.tail,
                }));
        }
    }

    private candyInterception(piece: [number, number], middleOne: [number, number]){
        let inBetween = (a: number, b: number, c: number) => a < c && b > c;

        return  inBetween(piece[0] - this.config.size/3, piece[0] + this.config.size + this.config.size/3, middleOne[0]) 
                    &&
                inBetween(piece[1] - this.config.size/3, piece[1] + this.config.size + this.config.size/3, middleOne[1]);
    }
    // checks for candy intercaption
    private checkCandyIntercaption(x: number, y: number): boolean{
        for(let i = 0; i < this.candies.length; i++){
            if( this.candyInterception([x, y], this.candies[i] ) ){
                this.gameCanvas.clearRect(this.candies[i][0], this.candies[i][1], this.config.size, this.config.size);
                this.candies = this.candies.filter( (val) => val !== this.candies[i] );
                this.extend = 5;
                this.score++;
                if(this.onScoreChange !== undefined)
                    this.onScoreChange(this.score);
                i--;
                this.getNewCandy();

                // return true; // because there could be only one candy at a time
            }
        }
        return false;
    }

    // generates new candy and returns array of two elements
    private getNewCandy(){
        let circleR = this.config.size / 3;
        let x: number;
        let y: number;
        do{
        x = Math.floor(Math.random() * (this.config.width  - 2 * circleR) + circleR);
        y = Math.floor(Math.random() * (this.config.height - 2 * circleR) + circleR);
        }while( !this.tail.every( piece => {
                    return  !this.candyInterception(piece, [x,y]);
                }) 
                    &&
                (this.opponent !== undefined ? !this.opponent!.getTail().tail.every( piece => {
                    return  !this.candyInterception(piece, [x,y]);
                }) 
                : 
                true) );

        this.candies.push([x, y]);
        if(this.socket !== undefined){
            this.score += Number(this.candies.length === 0);

            this.socket.send( JSON.stringify({ 
                ID: this.sessionID,
                candies: this.candies,
                score: this.score
            }) + "\0");
        }

        this.gameCanvas.fillStyle = this.config.candyColor;
        for(let i = 0;i < this.candies.length; i++){
            this.gameCanvas.beginPath();
            this.gameCanvas.arc(this.candies[i][0] + circleR, this.candies[i][1] + circleR, circleR, 0, 2 * Math.PI, false);
            this.gameCanvas.fill();
        }
    }
}