import { Fragment, React, useState } from "react";
import { Link } from "react-router-dom";
import { Col, Row, Card, Button, Image } from "react-bootstrap";
import FloatingLabel from "react-bootstrap/FloatingLabel";
import { Formik, ErrorMessage, Field, Form } from "formik"; 
import logoround from "../../../../assets/images/brand/wePairLogos/logoround.png";
import { signUpValSchema } from "./signUpValidation";
import { register } from "services/userService";
import "./signup.css";
import Swal from "sweetalert2";
import { ReactComponent as Slasheyeicon } from "../../../../assets/images/icons/slasheyeicon.svg";
import { ReactComponent as Eyeicon } from "../../../../assets/images/icons/eyeicon.svg";

const SignUp = () => {
  const [formData] = useState({
    FirstName: "",
    Mi: null,
    LastName: "",
    Email: "",
    Password: "",
    PasswordConfirm: "",
    avatarUrl: null,
    IsConfirmed: false,
    StatusId: 1,
  });

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const togglePasswordVisibility = () => {
    setShowPassword((prevState) => !prevState);
  };

  const toggleConfirmPasswordVisibility = () => {
    setShowConfirmPassword((prevState) => !prevState);
  };

  function submitForm(values, { setSubmitting }) {
    const onRegisterSuccess = (response) => {
      Swal.fire({
        title: "Thank You for Signing Up!",
        confirmButtonText: "Sign In?",
      }).then((result) => {
        if (result.isConfirmed) {
          Swal.fire(nav("/login", { state: values }));
        }
      });
    };
    const onRegisterError = (error) => {
      const errorMessage = error?.response?.data?.errors?.[0];
      if (errorMessage.includes("unique_email_constraint")) {
        Swal.fire({
          icon: "error",
          title: "Error",
          text: "An account with that email already exists.",
        });
      } else {
        Swal.fire({
          icon: "error",
          title: "Oops...",
          text: "Something went wrong! Try Again.",
        });
      }
    };
    _logger(values);
    register(values).then(onRegisterSuccess).catch(onRegisterError);

    setTimeout(() => {
      setSubmitting(false);
    }, 400);
  }

  return (
    <Fragment>
      <Formik
        enableReinitialize
        initialValues={formData}
        validationSchema={signUpValSchema}
        onSubmit={submitForm}>
        <Row className="align-items-center justify-content-center g-0 min-vh-100">
          <Col lg={5} md={5} className="py-8 py-xl-0">
            <Card>
              <Card.Body className="p-6">
                <div className="mb-4">
                  <Link to="/" className="cent">
                    <Image src={logoround} className="mb-4  logo-styling" alt="" />
                  </Link>
                  <h1 className="mb-1 fw-bold cent">Sign up</h1>
                  <div className="cent">
                    <span>
                      Already have an account?{" "}
                      <Link to="/authentication/sign-in" className="ms-1 ">
                        Sign in
                      </Link>
                    </span>
                  </div>
                </div>

                <Form>
                  <div className="form-group m-3">
                    <FloatingLabel controlId="floatingInputGrid" label="First Name">
                      <Field
                        type="text"
                        name="FirstName"
                        className="form-control"
                        placeholder=""
                      />
                    </FloatingLabel>
                    <ErrorMessage
                      name="FirstName"
                      component="div"
                      className="has-error"
                    />
                  </div>
                  <div className="form-group m-3">
                    <FloatingLabel controlId="floatingInputGrid" label="Middle Initial">
                      <Field
                        type="text"
                        name="Mi"
                        className="form-control"
                        placeholder=""
                      />
                    </FloatingLabel>
                    <ErrorMessage name="Mi" component="div" className="has-error" />
                  </div>

                  <div className="form-group m-3">
                    <FloatingLabel controlId="floatingInputGrid" label="Last Name">
                      <Field
                        type="text"
                        name="LastName"
                        className="form-control"
                        placeholder=""
                      />
                    </FloatingLabel>
                    <ErrorMessage name="LastName" component="div" className="has-error" />
                  </div>
                  <div className="form-group m-3">
                    <FloatingLabel controlId="floatingInputGrid" label="Email Address">
                      <Field
                        type="email"
                        name="Email"
                        className="form-control"
                        placeholder="name@example.com"
                      />
                    </FloatingLabel>
                    <ErrorMessage name="Email" component="div" className="has-error" />
                  </div>
                  <div className="form-group m-3">
                    <div className="password-wrapper">
                      <FloatingLabel controlId="floatingInputGrid" label="Password">
                        <Field
                          name="Password"
                          className="form-control"
                          placeholder=""
                          type={showPassword ? "text" : "password"}
                        />
                      </FloatingLabel>
                      <span className="password-icon" onClick={togglePasswordVisibility}>
                        {showPassword ? <Slasheyeicon /> : <Eyeicon />}
                      </span>
                    </div>
                    <ErrorMessage name="Password" component="div" className="has-error" />
                  </div>
                  <div className="form-group m-3">
                    <div className="password-wrapper">
                      <FloatingLabel
                        controlId="floatingInputGrid"
                        label="Confirm Password">
                        <Field
                          name="PasswordConfirm"
                          className="form-control"
                          placeholder=""
                          type={showConfirmPassword ? "text" : "password"}
                        />
                      </FloatingLabel>

                      <span
                        className="password-icon"
                        onClick={toggleConfirmPasswordVisibility}>
                        {showConfirmPassword ? <Slasheyeicon /> : <Eyeicon />}
                      </span>
                    </div>
                    <ErrorMessage
                      name="PasswordConfirm"
                      component="div"
                      className="has-error"
                    />
                  </div>
                  <Col lg={6} md={6} className="mb-0 d-grid gap-2 cent-2">
                    <Button
                      className="orange-rounded-btn"
                      variant="primary"
                      type="submit">
                      Sign Up
                    </Button>
                  </Col>
                </Form>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      </Formik>
    </Fragment>
  );
};

export default SignUp;
