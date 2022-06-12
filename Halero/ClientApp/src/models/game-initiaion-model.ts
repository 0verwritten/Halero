import { SnakeDirection } from "./snake-direction-model"

export type UserSessionData = {
    ID: string,
    Direction: SnakeDirection,
    Tail: Array<[number, number]>
}

export type GameSessionData = {
    ID: string,
    FirstUser: UserSessionData,
    SecondUser: UserSessionData,
    candies: Array<[number, number]>
    isOnPause: boolean
}