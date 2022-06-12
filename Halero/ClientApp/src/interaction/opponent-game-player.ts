import { SnakeGlobalConfig } from "../models/snake-config-model";
import { SnakeDirection } from "../models/snake-direction-model";
import { IGamePlayer } from "./game-player-interface";
import { MainPlayer } from "./main-game-player";

export class OpponentPlayer extends MainPlayer{
    constructor( 
                    configuration: SnakeGlobalConfig, 
                    gameCanvas: CanvasRenderingContext2D,
                    tail: Array<[number, number]> | null = null, 
                    direction: SnakeDirection | null = null, 
                    socket: WebSocket | undefined = undefined,
                    sessionID: string | undefined = undefined 
                ){
        super(configuration, gameCanvas, tail, direction, socket, sessionID, false);
    }


    setNewPosition(talo: Array<[number, number]>, direction: SnakeDirection){
        this.tail = talo;
        this.direction = direction;
    }
}