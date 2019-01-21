import { SerializeObject } from "../../types/serializable"
import { MSGameConf } from "./msgameconf"
import { MSUser } from "./msuser"

export interface MSRoom {
    /**
     * 아이디
     */
    id:string,
    /**
     * 생성 날짜 (타임스탬프)
     */
    created_at:number,
    /**
     * 방 설정
     */
    conf:MSRoomConf,
    /**
     * 들어있는 유저들
     */
    users:number[],
    /**
     * 게임중인가?
     */
    ingame:boolean,
}
export interface MSRoomConf {
    /**
     * 방이름
     */
    name:string,
    /**
     * 방장
     */
    king:number,
    /*
     * 게임 규칙
     */
    // rule:string,
    /**
     * 흑돌 유저
     */
    black:number,
    /**
     * 백돌 유저
     */
    white:number,
}
export interface MSRoomServer {
    invite:string,
    addr:string,
}