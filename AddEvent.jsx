import React, { useState, useEffect } from "react";
import { Modal } from 'react-bootstrap';
import { addEvent, uploadFile, updateEvent } from "../../services/eventService"
import { useNavigate } from "react-router-dom";
import { Formik, Form, useField } from 'formik';
import { eventValidationSchema } from "./validate";
import "./events.css"


function AddEvent(props) {
    const [newEvent, setNewEvent] = useState({ statusId: "", dateStart: "", dateEnd: "", location: [{ latitude: "", longitude: "", address: "", zipCode: "", city: "", state: "" }], name: "", headline: "", description: "", summary: "", slug: "" })
    const [id, setId] = useState("")
    const [eventAdded, setEventAdded] = useState(false);
    const { event, isOpen, onClose } = props;
    const nav = useNavigate()

    useEffect(() => {
        if (event && Object.keys(event).length > 0) {
            setId(event.id)
            const payload = { ...event, address: event.location[0].address, city: event.location[0].city, zipCode: event.location[0].zipCode, state: event.location[0].state, latitude: event.location[0].latitude, longitude: event.location[0].longitude };
            setNewEvent((prevState) => { return { ...prevState, ...payload }; });
        };
        if (id) {
            setEventAdded(true);
        }
    }, []);
    // console.log("NewEvent: ", newEvent)
    const fileSelected = (e) => {
        e.preventDefault()
        const file = e.target.files[0]
        const formData = new FormData();
        formData.append("file", file);
        uploadFile(formData).then(onFileUploadSuccess).catch(onFileUploadError)
    }
    const onFileUploadSuccess = response => {
        let url = response.data.items[0]
        setNewEvent((prevState) => ({ ...prevState, slug: url }))
    }
    const onFileUploadError = error => {
        console.log("onFileUploadError: ", error)
    }
    const MyTextInput = ({ label, ...props }) => {
        const [field, meta] = useField(props);
        return (
            <>
                <label htmlFor={props.id || props.name}>{label}</label>
                <input className="text-input" {...field} {...props} />
                {meta.touched && meta.error ? (
                    <div className="error">{meta.error}</div>
                ) : <div className="error-placeholder" />}
            </>
        );
    };

    const MySelect = ({ label, ...props }) => {
        const [field, meta] = useField(props);
        return (
            <div>
                <label htmlFor={props.id || props.name}>{label}</label>
                <select {...field} {...props} />
                {meta.touched && meta.error ? (
                    <div className="error">{meta.error}</div>
                ) : <div className="error-placeholder" />}
            </div>
        );
    };

    return (
        <Formik
            initialValues={newEvent}
            enableReinitialize
            validationSchema={eventValidationSchema}
            onSubmit={(values, { setSubmitting }) => {
                const finalValues = { ...values, location: [{ latitude: values.latitude, longitude: values.longitude, address: values.address, zipCode: values.zipCode, city: values.city, state: values.state }] };
                if (id || eventAdded) {
                    updateEvent(finalValues, finalValues.id)
                        .then(onUpdateEventSuccess)
                        .catch(onUpdateEventError);
                } else {
                    addEvent(finalValues)
                        .then(response => onAddEventSuccess(response, finalValues))
                        .catch(onAddEventError);
                }

                function onUpdateEventSuccess(response) {
                    nav("/events", { state: { payload: values, id: id } })
                }

                function onUpdateEventError(error) {
                    console.warn("onUpdateCompanyError: ", error)
                }
                function onAddEventSuccess(response, values) {
                    const id = response.data.item;
                    setNewEvent(prevState => {
                        return {
                            ...prevState,
                            ...values,
                            id: id
                        }
                    });
                    setEventAdded(true);
                    setId(id)
                    nav("/events", { state: { payload: values, id: id } })
                }

                function onAddEventError(error) {
                    console.log("error: ", error)
                }
                setTimeout(() => {
                    setSubmitting(false);
                }, 400);
            }}
            render={({ values }) => (
                <Modal className="evt-modal" show={isOpen} onHide={onClose} size="xl">
                    <Modal.Header closeButton>
                        <Modal.Title>{id ? `Update Event ID (${id})` : 'Add Event'}</Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <Form className="event">
                            <div className="row">
                                <div className="col-md-4">
                                    <MyTextInput
                                        label="Event Name"
                                        name="name"
                                        type="text"

                                    />
                                    <MyTextInput
                                        label="Headline"
                                        name="headline"
                                        type="text"
                                    />
                                    <MyTextInput
                                        label="Description"
                                        name="description"
                                        type="text"
                                    />
                                    <MyTextInput
                                        label="Summary"
                                        name="summary"
                                        type="text"
                                    />
                                    <MyTextInput
                                        label="Start Data"
                                        name="dateStart"
                                        type="datetime-local"
                                    />
                                    <MyTextInput
                                        label="End Date"
                                        name="dateEnd"
                                        type="datetime-local"
                                    />
                                    <MyTextInput
                                        label="Latitude"
                                        name="latitude"
                                        type="number"
                                    />
                                    <MyTextInput
                                        label="Longitude"
                                        name="longitude"
                                        type="number"
                                    />
                                </div>
                                <div className="col-md-4">
                                    <MyTextInput
                                        label="Address"
                                        name="address"
                                        type="text"
                                    />
                                    <MyTextInput
                                        label="City"
                                        name="city"
                                        type="text"
                                    />
                                    <MyTextInput
                                        label="State"
                                        name="state"
                                        type="text"
                                    />
                                    <MyTextInput
                                        label="Zip Code"
                                        name="zipCode"
                                        type="text"
                                    />

                                    <MyTextInput
                                        label="Media"
                                        name="slug"
                                        type="text"
                                    />
                                    <p>or File</p>
                                    <input type="file" multiple id="img" name="img" accept="image/png, image/jpeg, multipart/form-data" onChange={fileSelected} />
                                    <MySelect label="Status:&nbsp; " name="statusId" >
                                        <option value="">Select a Status</option>
                                        <option value="Active">Active</option>
                                        <option value="NotSet">NotSet</option>
                                        <option value="Flagged">Flagged</option>
                                        <option value="Deleted">Deleted</option>
                                    </MySelect>
                                </div>
                                <div className="col-md-4">
                                    <div className="more-card">
                                        <div className="item"><img className="event-img" src={values.slug} alt="Will show when there is a URL" /></div>
                                        <div className="item">
                                            <iframe title="moreMap" className="map mart-15" loading="lazy" allowFullScreen
                                                referrerPolicy="no-referrer-when-downgrade" src={}}>
                                            </iframe>
                                        </div>
                                    </div>
                                    <div className="item"><strong>Event Name:</strong> &nbsp;{values.name}</div>
                                    <div className="item"><strong>Headline:</strong> &nbsp;{values.headline}</div>
                                    <div className="item"><strong>Description:</strong> &nbsp;{values.description}</div>
                                    <div className="item"><strong>Summary:</strong> &nbsp;{values.summary}</div>
                                    <div className="item"><strong>When:</strong> &nbsp;{values.dateStart} to {values.dateEnd}</div>
                                    <div className="item"><strong>Where:</strong> &nbsp;{values.address}, {values.zipCode}</div>
                                    <div className="item"><strong>Event Status:</strong> &nbsp;{values.statusId}</div>
                                </div>
                            </div>
                            <button type="submit" className="aen-btn2">{id ? `Update Event ID (${id})` : 'Add'}</button>
                        </Form>
                    </Modal.Body>
                </Modal>
            )}
        />
    )
}


export default AddEvent;
