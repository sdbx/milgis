package models

import (
	"encoding/json"
	"errors"
)

type Result struct {
	Error *string `json:"error"`
	Value string  `json:"value"`
}

type TaskRequest struct {
	ID   string `json:"id"`
	Task Task   `json:"task"`
}

const (
	CreateRoomKind = "create-room"
	JoinRoomKind   = "join-room"
)

type Task interface {
	Kind() string
	Out() interface{}
	sealedTask()
}

func (r *TaskRequest) UnmarshalJSON(b []byte) error {
	obj := struct {
		ID   string `json:"id"`
		Task []byte `json:"task"`
	}{}
	err := json.Unmarshal(b, &obj)
	if err != nil {
		return err
	}

	r.ID = obj.ID

	obj2 := struct {
		Kind string `json:"kind"`
	}{}
	err = json.Unmarshal(obj.Task, &obj2)
	if err != nil {
		return err
	}

	switch obj2.Kind {
	case CreateRoomKind:
		r.Task = &CreateRoomTask{}
	case JoinRoomKind:
		r.Task = &JoinRoomTask{}
	default:
		return errors.New("unknown task kind")
	}

	return json.Unmarshal(obj.Task, r.Task)
}

func (r TaskRequest) MarshalJSON() ([]byte, error) {
	obj := struct {
		ID   string                 `json:"id"`
		Task map[string]interface{} `json:"task"`
	}{}
	obj.ID = r.ID
	buf, err := json.Marshal(r.Task)
	if err != nil {
		return nil, err
	}

	err = json.Unmarshal(buf, &obj.Task)
	if err != nil {
		return nil, err
	}

	obj.Task["kind"] = r.Task.Kind()
	return json.Marshal(obj)
}

type CreateRoomTask struct {
	Conf RoomConf `json:"conf"`
	User int      `json:"user"`
}

func (CreateRoomTask) Out() interface{} {
	return &CreateRoomResult{}
}

func (CreateRoomTask) Kind() string {
	return CreateRoomKind
}

func (CreateRoomTask) sealedTask() {}

type JoinRoomTask struct {
	Room string `json:"room"`
	User int    `json:"user"`
}

func (JoinRoomTask) Out() interface{} {
	return &JoinRoomResult{}
}

func (JoinRoomTask) Kind() string {
	return JoinRoomKind
}

func (JoinRoomTask) sealedTask() {}

type CreateRoomResult struct {
	Invite string `json:"invite"`
	Addr   string `json:"addr"`
}

type JoinRoomResult struct {
	Invite string `json:"invite"`
	Addr   string `json:"addr"`
}
