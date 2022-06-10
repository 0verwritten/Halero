import React from 'react';
import { toast } from 'react-toastify';
import { GameState } from '../models/game-state-model';
import { SnakeGlobalConfig } from '../models/snake-config-model';
import { IGamePlayer } from './game-player-interface';
import { MainPlayer } from './main-game-player';

type SoloPlaymentState = {
    gameConfig: SnakeGlobalConfig | null,
    playerScore: number,
    gameState: GameState
}

export class SoloPlayment extends React.Component<{}, SoloPlaymentState>{
    private canvas: React.RefObject<HTMLCanvasElement>;
    private player: IGamePlayer | null = null;

    constructor(props: {}){
        super(props);

        this.state ={
            gameConfig: {
                speed: 1,
                size: 10,
                width: 300,
                height: 300,
                candyColor: "#FF8888",
                frameRate: 1000 / 60 // 60 fps
            },
            playerScore: 0,
            gameState: GameState.NotStarted
        };

        this.canvas = React.createRef();
    }
    
    startGame(){
        this.player = new MainPlayer(
                this.state.gameConfig!,
                this.canvas.current?.getContext("2d")!
            );

        this.player.onScoreChange = score => this.setState( { playerScore: score } );
        this.player.onGameStateChange = state => {
            switch(state){
                case GameState.Waiting:
                    toast.info("Waiting, game is on pause", { autoClose: 1000 });
                    break;
                case GameState.Running:
                    toast.success("Game is now running again", { autoClose: 1000 });
                    break;
            }
            this.setState( { gameState: state } );
        }

        this.player.startGame();
    }

    getControllsInfo(){
        toast.info("To rotate the snake use:  ← ↑ → ↓ ")
        toast.info("To set pause use:         p");
        toast.info("To exit the game use:     q");
    }
    
    render(){
        require('./css/game.css');
        return (
            <div className="gamePlate singleUser">
                
                {
                    this.player != null && this.player.gameState !== GameState.NotStarted ?
                    <div className="userPlate yours">
                        <div className="score">
                            Score: <span>{ this.state.playerScore }</span>
                        </div>
                    </div>
                    : null
                }
                <canvas className={ "gamePlayCanvas " + ( this.player != null && this.player.gameState !== GameState.NotStarted ? "solo" : "" ) } width="300" height="300" ref={this.canvas}/>
                { 
                    this.player == null || this.player.gameState === GameState.NotStarted ?
                            <div className="introduction">
                                <button onClick={() => this.startGame()} className="startGameBtn">Start Game</button>
                                <button onClick={() => this.getControllsInfo()} className="controllsGameBtn">Controlls</button>
                            </div> 
                        :
                            null
                }
            </div>
        );
    }
}